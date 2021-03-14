using ArbitralSystem.Connectors.Core;
using JetBrains.Annotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.Common
{
    [UsedImplicitly]
    internal class PublicMarketServiceConnectionInfo
    {
        public string BaseUrl { get; set; }
        public int DefaultTimeOutInMs { get; set;}
    }
}