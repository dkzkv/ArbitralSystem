using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary
{
    internal class Balance: IBalance
    {
        public Exchange Exchange { get; set; }
        public string Currency { get; set; }
        public decimal Total { get; set; }
        public decimal Available { get; set; }
    }
}