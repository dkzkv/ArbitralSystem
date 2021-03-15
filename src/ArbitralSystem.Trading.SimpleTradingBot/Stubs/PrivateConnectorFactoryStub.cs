using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    internal class PrivateConnectorFactoryStub : IPrivateConnectorFactory
    {
        private readonly IPrivateConnector _binanceStub;
        private readonly IPrivateConnector _huobiStub;

        public PrivateConnectorFactoryStub(SimpleBotSettings botSettings)
        {
            Preconditions.CheckNotNull(botSettings);
            _binanceStub = new PrivateBinanceConnectorStub(botSettings);
            _huobiStub = new PrivateHuobiConnectorStub(botSettings);
        }

        public IPrivateConnector GetInstance(Exchange exchange)
        {
            switch (exchange)
            {
                case Exchange.Binance:
                    return _binanceStub;
                case Exchange.Huobi:
                    return _huobiStub;
                default: throw new StubException("Wrong exchange");
            }
        }
    }
}