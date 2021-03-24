using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Distributor.Core.Test
{
    internal class TestOrderBookEntry : IOrderbookEntry
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}