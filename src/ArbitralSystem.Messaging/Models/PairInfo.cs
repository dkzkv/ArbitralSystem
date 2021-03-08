using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Messaging.Models
{
    public class PairInfo
    {
        public int PairId { get; set; }
        public Exchange Exchange { get ; set; }
        public string UnificatedPairName { get; set; }
        public string ExchangePairName { get; set; }
    }
}