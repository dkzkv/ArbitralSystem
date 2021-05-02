using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Distributor.MarketInfoAggregatorService.Common;
using ArbitralSystem.Distributor.MarketInfoAggregatorService.Consumers;
using ArbitralSystem.Distributor.MarketInfoAggregatorService.Services;
using ArbitralSystem.Messaging.Common;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Messaging.Options;
using ArbitralSystem.Service.Core;
using MassTransit;
using MassTransit.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArbitralSystem.Distributor.MarketInfoAggregatorService
{
    internal class Program
    {
        private static readonly string EnvironmentBuildName = Environment.GetEnvironmentVariable("BUILD_ENVIRONMENT");
        private static readonly string ApplicationName = "MarketInfoAggregator";

        static async Task Main()
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
                await AggregatorService(configuration, loggerWrapper);
            }
            catch (Exception e)
            {
                Console.WriteLine($"_FINAL_MES: {e.Message}");
                loggerWrapper.Fatal(e, "Unexpected fatal error!");
                throw;
            }
            finally
            {
                loggerWrapper.Dispose();
            }
        }

        static async Task AggregatorService(IConfigurationRoot configuration, ILogger loggerWrapper)
        {
            IConnectionOptions mqOptions = configuration.GetSection("RabbitMQ:Connection")
                .Get<ConnectionOptions>();

            var aggregatorOptions = configuration.GetSection("MarketInfoAggregator:Options")
                .Get<MarketInfoAggregatorOptions>();

            var aggregatorLimits = configuration.GetSection("MarketInfoAggregator:Options:Limits")
                .Get<AggregatorOptions>();

            var settingsLogger = new SettingsLogger(loggerWrapper);

            #region Log settings

            var hideRule = new PasswordRegexRule()
            {
                RegexPattern = RegexPasswordPatterns.RabbitMq,
                NameOfProperty = nameof(mqOptions.Host),
                ObjectType = mqOptions.GetType()
            };
            var passwordHideRules = new Dictionary<Type, PasswordRegexRule>();
            passwordHideRules.Add(hideRule.ObjectType, hideRule);

            settingsLogger.PasswordHiddenInfo(passwordHideRules,
                mqOptions,
                aggregatorOptions,
                aggregatorLimits);

            #endregion

            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) => { configApp.AddConfiguration(configuration); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(mqOptions);
                    services.AddSingleton(loggerWrapper);
                    services.AddSingleton(configuration);
                    services.AddSingleton(aggregatorLimits);
                    services.AddSingleton<IBusControlFactory, BusControlFactory>();

                    services
                        .AddSingleton<ITimeLimitedAggregator<IOrderBookMessage>,
                            TimeLimitedAggregator<IOrderBookMessage>>();

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<OrderBookConsumer>();
                        MessageCorrelation.UseCorrelationId<IOrderBookMessage>(o => o.CorrelationId);

                        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            cfg.Host(new Uri(mqOptions.Host), h => { });

                            cfg.ReceiveEndpoint(Constants.Queues.OrderBookAggregatorConsumer, e =>
                            {
                                e.PrefetchCount = aggregatorOptions.PrefetchCount;
                                e.ConfigureConsumer<OrderBookConsumer>(provider);
                            });
                        }));
                    });
                    services.AddHostedService<AggregatorService>();
                })
                .UseConsoleLifetime();

            await hostBuilder.RunConsoleAsync();
        }
    }
}