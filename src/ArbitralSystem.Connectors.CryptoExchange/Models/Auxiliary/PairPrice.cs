using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models
{
    internal class PairPrice : IPairPrice
    {
        public Exchange Exchange { get ; set; }
        public string ExchangePairName { get ; set; }
        public decimal? Price { get ; set; }
    }
}