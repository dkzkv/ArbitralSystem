using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;
using ArbitralSystem.Trading.SimpleTradingBot.Strategies;
using ArbitralSystem.Trading.SimpleTradingBot.Stubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ArbitralSystem.Trading.SimpleTradingBot
{
    class Program
    {
        private static readonly string EnvironmentBuildName = Environment.GetEnvironmentVariable("BUILD_ENVIRONMENT");
        private static readonly string ApplicationName = "SimpleTradingBot";

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
                await SimpleTradingBotService(configuration, loggerWrapper);
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

        static async Task SimpleTradingBotService(IConfigurationRoot configuration, ILogger loggerWrapper)
        {
            var connectivity = configuration.GetSection("ExchangeConnectivity")
                .Get<ExchangeConnectivity[]>()
                .ToArray<IPrivateExchangeSettings>();

            var strategySettings = configuration.GetSection("StrategySettings")
                .Get<StrategySettings>();
            var botSettings = configuration.GetSection("BotSettings")
                .Get<SimpleBotSettings>();

            loggerWrapper.Information("Bot settings : {@botSettings}",botSettings);
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, configApp) => { configApp.AddConfiguration(configuration); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(loggerWrapper);
                    services.AddSingleton(connectivity);
                    services.AddSingleton(botSettings);
                    services.AddSingleton(strategySettings);
                    services.AddSingleton<IDtoConverter>(new CryptoExchangeConverter());

                    services.AddSingleton<IOrderGeneratorStrategy, SimpleMarketGenerator>();
                    services.AddTransient<IPublicConnectorFactory, CryptoExPublicConnectorFactory>();
                    services.AddTransient<IOrderBookDistributerFactory>(provider => new CryptoExOrderBookDistributerFactory(
                        provider.GetService<IDtoConverter>(),
                        provider.GetService<ILogger>()));
                    services.AddTransient<IAccountConnectorFactory, CryptoExAccountConnectorFactory>();
                    
                    if (botSettings.IsTestMode)
                        services.AddTransient<IPrivateConnectorFactory, PrivateConnectorFactoryStub>();
                    else
                        services.AddTransient<IPrivateConnectorFactory, CryptoExPrivateConnectorFactory>();


                    services.AddHostedService<SimpleTradingBotService>();
                })
                .UseConsoleLifetime();

            await hostBuilder.RunConsoleAsync();
        }
    }
}