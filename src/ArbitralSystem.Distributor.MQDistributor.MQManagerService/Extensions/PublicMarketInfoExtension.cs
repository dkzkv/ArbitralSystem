using ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector;
using ArbitralSystem.Connectors.Core;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.Common;
using Microsoft.Extensions.DependencyInjection;

namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.Extensions
{
    internal static class PublicMarketInfoExtension
    {
        public static void AddPublicMarketInfo(this IServiceCollection services, PublicMarketServiceConnectionInfo options)
        {
            services.AddScoped<IPublicMarketInfoConnector>(connector 
                => new PublicMarketInfoConnector(options.BaseUrl, new ConnectionOptions(options.DefaultTimeOutInMs)));
        }
    }
}