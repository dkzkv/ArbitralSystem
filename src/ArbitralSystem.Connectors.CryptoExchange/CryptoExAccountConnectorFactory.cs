using System;
using System.Linq;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    public class CryptoExAccountConnectorFactory : IAccountConnectorFactory
    {
        private readonly IPrivateExchangeSettings[] _credentials;

        public CryptoExAccountConnectorFactory(IPrivateExchangeSettings[] credentials)
        {
            Preconditions.CheckNotNull(credentials);
            if (!credentials.Any())
                throw new ArgumentException("Credentials for factory is empty.");
            
            if (credentials.GroupBy(o => o.Exchange).Any(o => o.Count() > 1))
                throw new ArgumentException("Ambiguity credential settings.");

            _credentials = credentials;
        }

        public IExtraConnector GetInstance(Exchange exchange)
        {
            var exchangeCredentials = _credentials.FirstOrDefault(o => o.Exchange == exchange);
            if(exchangeCredentials == null)
                    throw new InvalidOperationException($"There is no credentials for: {exchange}");
            
            switch (exchange)
            {
                case Exchange.Binance:
                    return new BinanceExtraConnector(exchangeCredentials);
                case Exchange.Huobi:
                    return new HuobiExtraConnector(exchangeCredentials);
                default: throw new NotSupportedException(
                    $"Not supported {Enum.GetName(typeof(Exchange), exchange)} exchange for {nameof(CryptoExAccountConnectorFactory)}");
            }
        }
    }
}