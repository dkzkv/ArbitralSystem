using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Account;

namespace ArbitralSystem.Trading.SimpleTradingBot.Strategies
{
    internal class OrderBookContext
    {
        public OrderBookContext(IPairInfo pairInfo, IOrderBook orderBook, IPairCommission pairCommission)
        {
            PairInfo = pairInfo;
            OrderBook = orderBook;
            PairCommission = pairCommission;
        }

        public IPairInfo PairInfo { get; }
        public IOrderBook OrderBook { get; }
        public IPairCommission PairCommission { get; }
    }
}