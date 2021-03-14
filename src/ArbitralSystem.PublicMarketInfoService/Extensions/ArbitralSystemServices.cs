using System;
using ArbitralSystem.Connectors.CoinEx;
using ArbitralSystem.Connectors.Core;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.PublicMarketInfoService.Domain.Interfaces;
using ArbitralSystem.PublicMarketInfoService.Domain.Services;
using ArbitralSystem.PublicMarketInfoService.Persistence.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArbitralSystem.PublicMarketInfoService.Extensions
{
    internal static class ArbitralSystemServices
    {
        private static string DomainAssembly => "ArbitralSystem.PublicMarketInfoService.Domain";
        private static string PersistenceAssembly => "ArbitralSystem.PublicMarketInfoService.Persistence";
        
        public static void AddArbitralSystemServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(AppDomain.CurrentDomain.Load(DomainAssembly),
                AppDomain.CurrentDomain.Load(PersistenceAssembly));
            
            services.AddScoped<IDtoConverter, CryptoExchangeConverter>();
            services.AddScoped<ICoinExConnector, CoinExConnector>();
            services.AddScoped<IPublicConnectorFactory,CryptoExPublicConnectorFactory>();
            
            services.AddScoped<IPairPricesRepository, PairPricesRepository>();
            services.AddScoped<IPairInfoRepository, PairInfoBaseRepository>();
        }
    }
}