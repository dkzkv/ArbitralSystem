using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Account;

namespace ArbitralSystem.Trading.SimpleTradingBot.Strategies
{
    internal class OrderBookContext
    {
        public OrderBookContext(IPairInfo pairInfo, IDistributorOrderBook distributorOrderBook, IPairCommission pairCommission, BotBalance balance)
        {
            PairInfo = pairInfo;
            DistributorOrderBook = distributorOrderBook;
            PairCommission = pairCommission;
            Balance = balance;
        }

        public IPairInfo PairInfo { get; }
        public IDistributorOrderBook DistributorOrderBook { get; }
        public IPairCommission PairCommission { get; }
        public BotBalance Balance { get; }
    }
}