using System;
using System.Linq;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    internal class BasePrivateConnectorStub
    {
        public string BaseCurrency { get; }
        public string QuoteCurrency { get; }
        public BasePrivateConnectorStub(SimpleBotSettings botSettings)
        {
            var splitPair = botSettings.UnificatedPairName.Split('/');
            if(splitPair.Count() != 2)
                throw new ArgumentException($"Invalid pair format: {botSettings.UnificatedPairName} {{Base}}/{{Quote}}, for stub private connector");
            BaseCurrency = splitPair[0];
            QuoteCurrency = splitPair[1];
        }
    }
}