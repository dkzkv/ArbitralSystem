using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models
{
    public class ArbitralPairInfoFilterFilter
    {
        public string ExchangePairName { get; set; }
        public string BaseCurrency { get; set;}
        public string QuoteCurrency { get; set;}
        public Exchange? Exchange { get; set; }
        public int? Count { get; set; }
        public int? Offset { get; set; }
    }
}