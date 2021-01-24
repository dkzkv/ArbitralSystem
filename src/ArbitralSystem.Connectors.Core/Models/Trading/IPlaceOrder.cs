using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models.Trading
{
    public interface IPlaceOrder : IExchange
    {
        string Symbol { get; }
        OrderSide OrderSide { get; }
        OrderType OrderType { get; }
        decimal? Price { get; }
        decimal? Quantity { get; }
        string ClientOrderId { get; }
    }

    public interface ICancelOrder : IExchange
    {
        string Symbol { get; }
        string Id { get; }
        string ClientOrderId { get; }
    }
    

    public interface IOrder : IPlaceOrder
    {    
        string Id { get; }
        OrderStatus Status { get; }
        DateTimeOffset CreatedAt { get; }
    }

    public interface IOrderRequest
    {
        public string Symbol { get; }
        public string OrderId { get; }
        public string ClientOrderId { get; }
    }
}