using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Trading.SimpleTradingBot.Settings
{
    public class SimpleBotSettings
    {
        public Exchange FirstExchange { get; set; }
        public Exchange SecondExchange { get; set; }
        public string UnificatedPairName { get; set; }
    }
}