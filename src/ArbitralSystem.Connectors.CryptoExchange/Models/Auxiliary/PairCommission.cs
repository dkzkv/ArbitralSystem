using ArbitralSystem.Connectors.Core.Models.Account;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary
{
    internal class PairCommission : IPairCommission
    {
        public Exchange Exchange { get; set; }
        public string ExchangePairName { get; set; }
        public decimal TakerPercent { get; set; }
        public decimal MakerPercent { get; set; }
    }
}