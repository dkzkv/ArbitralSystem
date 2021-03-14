using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models.Trading
{
    public interface IPlaceOrder : IExchange
    {
        string ExchangePairName { get; }
        OrderSide OrderSide { get; }
        OrderType OrderType { get; }
        decimal? Price { get; }
        decimal? Quantity { get; }
        string ClientOrderId { get; }
    }

    public interface ICancelOrder : IExchange
    {
        string Symbol { get; }
        string OrderId { get; }
        string ClientOrderId { get; }
    }
    

    public interface IOrder : IPlaceOrder
    {    
        string Id { get; }
        bool IsActive { get; }
        DateTimeOffset CreatedAt { get; }
    }

    public interface IOrderRequest
    {
        public string Symbol { get; }
        public string OrderId { get; }
        public string ClientOrderId { get; }
    }
    
    public interface IBalance : IExchange
    {
        string Currency { get; }
        decimal Total { get; }
        decimal Available { get; }
    }
}