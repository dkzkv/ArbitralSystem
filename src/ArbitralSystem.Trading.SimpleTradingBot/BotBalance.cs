using ArbitralSystem.Connectors.Core.Models.Trading;

namespace ArbitralSystem.Trading.SimpleTradingBot
{
    public class BotBalance
    {
        public BotBalance(IBalance baseBalance, IBalance quoteBalance)
        {
            BaseBalance = baseBalance;
            QuoteBalance = quoteBalance;
        }

        public BotBalance Update(IBalance baseBalance, IBalance quoteBalance)
        {
            BaseBalance = baseBalance;
            QuoteBalance = quoteBalance;
            return this;
        }

        public IBalance BaseBalance { get; private set; }
        public IBalance QuoteBalance { get; private set; }
    }
}