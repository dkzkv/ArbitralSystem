using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Connectors.CryptoExchange.Models
{
    internal class OrderbookEntry : IOrderbookEntry
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}