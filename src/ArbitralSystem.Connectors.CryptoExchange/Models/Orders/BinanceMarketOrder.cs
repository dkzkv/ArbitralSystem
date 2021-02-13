using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Orders
{
    public abstract class BaseMarketOrder : IPlaceOrder
    {
        protected BaseMarketOrder(string symbol, OrderSide orderSide, decimal quantity, string clientOrderId)
        {
            ExchangePairName = symbol;
            OrderSide = orderSide;
            Quantity = quantity;
            ClientOrderId = clientOrderId;
            OrderType = OrderType.Market;
            Price = null;
        }

        public abstract Exchange Exchange { get; }
        public string ExchangePairName { get;  }
        public OrderSide OrderSide { get;  }
        public OrderType OrderType { get;  }
        public decimal? Price { get; }
        public decimal? Quantity { get; }
        public string ClientOrderId { get;  }
    }
    
    public abstract class BaseLimitOrder : IPlaceOrder
    {
        protected BaseLimitOrder(string symbol, OrderSide orderSide, decimal quantity, decimal price , string clientOrderId)
        {
            ExchangePairName = symbol;
            OrderSide = orderSide;
            Quantity = quantity;
            ClientOrderId = clientOrderId;
            OrderType = OrderType.Limit;
            Price = price;
        }

        public abstract Exchange Exchange { get; }
        public string ExchangePairName { get;  }
        public OrderSide OrderSide { get;  }
        public OrderType OrderType { get;  }
        public decimal? Price { get; }
        public decimal? Quantity { get; }
        public string ClientOrderId { get;  }
    }
    
    public class MarketOrder : BaseMarketOrder
    {
        public MarketOrder(string symbol, OrderSide orderSide, decimal quantity, string clientOrderId, Exchange exchange) 
            : base(symbol, orderSide, quantity, clientOrderId)
        {
            Exchange = exchange;
        }

        public override Exchange Exchange {get;}
    }
    
    public class LimitOrder : BaseLimitOrder
    {
        public LimitOrder(string symbol, OrderSide orderSide, decimal quantity, decimal price, string clientOrderId, Exchange exchange) 
            : base(symbol, orderSide, quantity, price, clientOrderId )
        {
            Exchange = exchange;
        }

        public override Exchange Exchange { get; }
    }
}