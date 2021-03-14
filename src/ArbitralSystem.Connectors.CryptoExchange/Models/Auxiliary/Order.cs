using System;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary
{
    internal class Order : IOrder
    {
        public Exchange Exchange { get; set; }
        public string ExchangePairName { get; set; }
        public OrderSide OrderSide { get; set; }
        public OrderType OrderType { get; set; }
        public decimal? Price { get; set; }
        public decimal? Quantity { get; set; }
        public string ClientOrderId { get; set; }
        public string Id { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}