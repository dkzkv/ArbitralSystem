using System;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Orders
{
    public class CancelOrder : ICancelOrder
    {
        public CancelOrder(Exchange exchange, string symbol, string orderId, string clientOrderId)
        {
            if(exchange == Exchange.Undefined)
                throw new ArgumentException("Exchange is undefined for order cancellation.");
            
            if(string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(clientOrderId))
                throw new ArgumentException("id, or client order id should be filled to cancel order.");
            
            Exchange = exchange;
            Symbol = symbol;
            OrderId = orderId;
            ClientOrderId = clientOrderId;
        }
        
        public Exchange Exchange { get; }
        public string Symbol { get;  }
        public string OrderId { get;  }
        public string ClientOrderId { get; }
    }

    public class OrderRequest : IOrderRequest
    {
        public OrderRequest(string symbol, string orderId, string clientOrderId)
        {
            if(string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(clientOrderId))
                throw new ArgumentException("id, or client order id should be filled to cancel order.");
            
            Symbol = symbol;
            OrderId = orderId;
            ClientOrderId = clientOrderId;
        }

        public string Symbol { get; set; }
        public string OrderId { get; set; }
        public string ClientOrderId { get; set; }
    }
}