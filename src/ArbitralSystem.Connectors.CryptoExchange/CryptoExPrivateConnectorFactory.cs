using System;
using System.Linq;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Connectors.CryptoExchange.Models.TradingSettings;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    public class CryptoExPrivateConnectorFactory : IPrivateConnectorFactory
    {
        private readonly IPrivateExchangeSettings[] _privateSettings;
        private readonly IConverter _converter;

        public CryptoExPrivateConnectorFactory(IPrivateExchangeSettings[] privateSettings)
        {
            Preconditions.CheckNotNull(privateSettings);
            if (!privateSettings.Any())
                throw new ArgumentException("Credentials for factory is empty.");

            if (privateSettings.GroupBy(o => o.Exchange).Any(o => o.Count() > 1))
                throw new ArgumentException("Ambiguity credential settings.");
            _converter = new CryptoExchangeConverter();
            _privateSettings = privateSettings;
        }

        public IPrivateConnector GetInstance(Exchange exchange)
        {
            var privateSettings = _privateSettings.FirstOrDefault(o => o.Exchange == exchange);
            if (privateSettings is null)
                throw new ArgumentException($"Credentials for Exchange: {exchange} not found.");

            switch (exchange)
            {
                case Exchange.Binance:
                    return new BinancePrivateConnector(privateSettings, _converter);

                case Exchange.Huobi:
                {
                    return new HuobiPrivateConnector(privateSettings.AccountId ?? throw new ArgumentNullException("Account Id for huobi exchange is null"),
                        privateSettings,
                        _converter);
                }

                default:
                    throw new NotSupportedException(
                        $"Not supported {Enum.GetName(typeof(Exchange), exchange)} exchange for {nameof(CryptoExPrivateConnectorFactory)}");
            }
        }
    }
}