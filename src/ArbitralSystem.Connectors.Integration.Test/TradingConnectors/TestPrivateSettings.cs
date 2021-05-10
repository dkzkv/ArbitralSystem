using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.Integration.Test.TradingConnectors
{
    [UsedImplicitly]
    internal class TestPrivateSettings : IPrivateExchangeSettings
    {
        public Exchange Exchange { get; set; }
        public long? AccountId { get; set; }
        
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }
    }
}