using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    internal class StubBalance : IBalance
    {
        public Exchange Exchange { get; set; }
        public string Currency { get; set; }
        public decimal Total { get; set; }
        public decimal Available { get; set; }
    }
}