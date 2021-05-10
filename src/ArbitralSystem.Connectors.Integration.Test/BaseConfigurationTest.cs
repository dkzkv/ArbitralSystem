using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Integration.Test.TradingConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.Extensions.Configuration;

namespace ArbitralSystem.Connectors.Integration.Test
{
    internal abstract class BaseConfigurationTest
    {
        private static readonly string EnvironmentBuildName = Environment.GetEnvironmentVariable("BUILD_ENVIRONMENT");
        private readonly IConfigurationRoot _configuration;
        public BaseConfigurationTest()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentBuildName}.json", true)
                .AddEnvironmentVariables()
                .Build();
        }

        protected TestPrivateSettings GetCredentials(Exchange exchange)
        {
            if (exchange == Exchange.Undefined)
                throw new ArgumentException("Exchange can't be undefined");
            var credentials = _configuration.GetSection("ExchangeCredentials").Get<IEnumerable<TestPrivateSettings>>();
            var concreteCredentials = credentials.FirstOrDefault(o => o.Exchange.Equals(exchange));

            if (concreteCredentials is null)
                throw new ArgumentException($"Settings for {exchange} exchange not defined in appsettings file.");
            return concreteCredentials;
        }
    }
}