using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.CryptoExchange;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Distributor.Core.Common;
using ArbitralSystem.Distributor.Core.Interfaces;
using ArbitralSystem.Distributor.Core.Jobs;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Common;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Common.Settings;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Consumers;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Jobs;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging;
using ArbitralSystem.Messaging.Common;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Messaging.Options;
using ArbitralSystem.Service.Core;
using AutoMapper;
using GreenPipes;
using JetBrains.Annotations;
using MassTransit;
using MassTransit.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService
{
    [UsedImplicitly]
    class Program
    {
        private static readonly string EnvironmentBuildName = Environment.GetEnvironmentVariable("BUILD_ENVIRONMENT");
        private static readonly string ApplicationName = "MqOrderBookDistributor";
        
        private const string CoreLayer = "ArbitralSystem.Distributor.Core";

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentBuildName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var loggerWrapper = new LoggerFactory(configuration).GetInstance();
            loggerWrapper = loggerWrapper.ForContext("Application", ApplicationName);

            try
            {
                await MqOrderBookDistributerService(configuration, loggerWrapper);
            }
            catch (OperationCanceledException)
            {
                loggerWrapper.Warning("Operation was canceled");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                loggerWrapper.Fatal(e, "Unexpected fatal error!");
                throw;
            }
            finally
            {
                loggerWrapper.Dispose();
            }
        }

        static async Task MqOrderBookDistributerService(IConfigurationRoot configuration, ILogger loggerWrapper)
        {
            var serviceOptions = configuration.GetSection("DistributorServerSettings").Get<DistributorServerSettings>();
            IConnectionOptions mqOptions = configuration.GetSection("RabbitMQ:Connection").Get<ConnectionOptions>();
            IDistributerOptions distributerOptions = configuration.GetSection("OrderBookDistributer:Options").Get<DistributerOptions>();
            var distributionOptions = new DistributionOptions(configuration.GetValue<int?>("OrderBookDistributer:Options:TrimLimit"));

            var settingsLogger = new SettingsLogger(loggerWrapper);
            var hideRule = new PasswordRegexRule()
            {
                RegexPattern = RegexPasswordPatterns.RabbitMq,
                NameOfProperty = nameof(mqOptions.Host),
                ObjectType = mqOptions.GetType()
            };
            
            var passwordHideRules = new Dictionary<Type, PasswordRegexRule>();
            passwordHideRules.Add(hideRule.ObjectType, hideRule);

            settingsLogger.PasswordHiddenInfo(passwordHideRules,
                serviceOptions,
                mqOptions);
            
            loggerWrapper.Information($"Frequency: {distributerOptions.Frequency}, Trim limit: {distributionOptions.TrimOrderBookDepth}");
            
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) => { configApp.AddConfiguration(configuration); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddAutoMapper(Assembly.GetExecutingAssembly(),
                        AppDomain.CurrentDomain.Load(CoreLayer));

                    services.AddSingleton(serviceOptions);
                    services.AddSingleton(loggerWrapper);
                    services.AddSingleton(configuration);
                    services.AddSingleton(distributerOptions);
                    services.AddSingleton(distributionOptions);

                    services.AddSingleton<JobManager>();
                    services.AddTransient<OrderBookDistributorJob>();
                    services.AddTransient<IOrderBookPublisher, OrderBookPublisher>();

                    services.AddSingleton<IDtoConverter, CryptoExchangeConverter>();
                    services.AddTransient<IOrderBookDistributerFactory, CryptoExOrderBookDistributerFactory>();

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<OrderBookDistributorConsumer>();
                        x.AddConsumer<JobCancellationConsumer>();
                        MessageCorrelation.UseCorrelationId<IStartOrderBookDistribution>(o => o.CorrelationId);
                        MessageCorrelation.UseCorrelationId<IStopOrderBookDistribution>(o => o.CorrelationId);
                        
                        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            // RoundRobin
                            cfg.ReceiveEndpoint(Constants.Queues.MQOrderBookDistributorPrefix, e =>
                            {
                                e.UseMessageRetry(r => r.Immediate(int.MaxValue));
                                e.PrefetchCount = serviceOptions.MaxWorkersCount;
                                e.ConfigureConsumer<OrderBookDistributorConsumer>(provider);
                            });
                            
                            // Fan-out
                            cfg.ReceiveEndpoint(Constants.Queues.MQOrderBookDistributorCancellationPrefix + serviceOptions.ServerName, e =>
                            {
                                e.PrefetchCount = 200;
                                e.ConfigureConsumer<JobCancellationConsumer>(provider);
                            });
                        }));
                    });
                    services.AddHostedService<OrderBookDistributorService>();
                })
                .UseConsoleLifetime();

            await hostBuilder.RunConsoleAsync();
        }
    }
}