using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ArbitralSystem.Connectors.Integration.Test
{
    public abstract class BaseConfigurationTest
    {
        private static readonly string EnvironmentBuildName = Environment.GetEnvironmentVariable("BUILD_ENVIRONMENT");

        public BaseConfigurationTest()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{EnvironmentBuildName}.json", true)
                .AddEnvironmentVariables()
                .Build();
        }

        protected IConfigurationRoot Configuration { get; }
    }
}