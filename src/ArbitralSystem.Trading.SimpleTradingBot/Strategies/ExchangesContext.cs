using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Trading.SimpleTradingBot.Strategies
{
    public class ExchangesContext
    {
        public ExchangesContext(BotBalance firstBalance, BotBalance secondBalance, IDistributorOrderBook firstDistributorOrderBook, IDistributorOrderBook secondDistributorOrderBook)
        {
            FirstBalance = firstBalance;
            SecondBalance = secondBalance;
            FirstDistributorOrderBook = firstDistributorOrderBook;
            SecondDistributorOrderBook = secondDistributorOrderBook;
        }

        public BotBalance FirstBalance { get; }
        public BotBalance SecondBalance{ get; }
        public IDistributorOrderBook FirstDistributorOrderBook{ get; }
        public IDistributorOrderBook SecondDistributorOrderBook{ get; }
    }
}