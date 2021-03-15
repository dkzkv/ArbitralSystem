using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    internal class StubMarketOrder : IPlaceOrder
    {
        public StubMarketOrder(Exchange exchange, string exchangePairName, OrderSide orderSide, decimal? price, decimal? quantity, string clientOrderId)
        {
            Exchange = exchange;
            ExchangePairName = exchangePairName;
            OrderSide = orderSide;
            OrderType = OrderType.Market;
            Price = price;
            Quantity = quantity;
            ClientOrderId = clientOrderId;
        }

        public Exchange Exchange { get; set; }
        public string ExchangePairName { get; set; }
        public OrderSide OrderSide { get; set; }
        public OrderType OrderType { get; set; }
        public decimal? Price { get; set; }
        public decimal? Quantity { get; set; }
        public string ClientOrderId { get; set; }
    }
}