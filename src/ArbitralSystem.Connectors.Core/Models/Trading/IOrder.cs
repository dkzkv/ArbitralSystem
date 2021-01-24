using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models.Trading
{
    public interface IOrder
    {
        string Symbol { get; }
        OrderSide OrderSide { get; }
        OrderType OrderType { get; }
        decimal Price { get; }
        decimal? Quantity { get; }
    }
    
    public interface IPlacedOrder : IOrder
    {
        string Id { get; }
        //status
        DateTimeOffset CreatedAt { get; }
    }
}