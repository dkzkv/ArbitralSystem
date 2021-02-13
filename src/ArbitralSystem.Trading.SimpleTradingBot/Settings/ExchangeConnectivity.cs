using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Trading.SimpleTradingBot.Settings
{
    internal class ExchangeConnectivity : IPrivateExchangeSettings
    {
        public Exchange Exchange { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }
        public long? AccountId { get; set; }
    }
}