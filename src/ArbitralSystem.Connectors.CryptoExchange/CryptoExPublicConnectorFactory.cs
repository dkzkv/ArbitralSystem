using System;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Bitfinex;
using ArbitralSystem.Connectors.CoinEx;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;
using BitfinexConnector = ArbitralSystem.Connectors.CryptoExchange.PublicConnectors.BitfinexConnector;
using CoinExConnector = ArbitralSystem.Connectors.CryptoExchange.PublicConnectors.CoinExConnector;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    public class CryptoExPublicConnectorFactory : IPublicConnectorFactory
    {
        private readonly ICoinExConnector _coinExConnector;
        private readonly IBitfinexConnector _bitfinexConnector;
        private readonly IDtoConverter _converter;
        private readonly ILogger _logger;

        public CryptoExPublicConnectorFactory(
            [NotNull] IDtoConverter converter,
            [NotNull] ILogger logger,
            [CanBeNull] ICoinExConnector coinExConnector = null,
            [CanBeNull] IBitfinexConnector bitfinexConnector = null)
        {
            _bitfinexConnector = bitfinexConnector ?? new Bitfinex.BitfinexConnector();
            _coinExConnector = coinExConnector ?? new CoinEx.CoinExConnector();
            _converter = converter;
            _logger = logger;
        }

        public IPublicConnector GetInstance(Exchange exchange)
        {
            switch (exchange)
            {
                case Exchange.Binance:
                    return new BinanceConnector(_converter);

                case Exchange.Bittrex:
                    return new BittrexConnector(_converter);

                case Exchange.CoinEx:
                    return new CoinExConnector(_coinExConnector, _converter, _logger);

                case Exchange.Huobi:
                    return new HuobiConnector(_converter);

                case Exchange.Kraken:
                    return new KrakenConnector(_converter);

                case Exchange.Kucoin:
                    return new KucoinConnector(_converter);

                case Exchange.Bitmex:
                    return new BitmexConnector(_converter);

                case Exchange.Bitfinex:
                    return new BitfinexConnector(_bitfinexConnector, _converter);

                default:
                    throw new NotSupportedException(
                        $"Not supported {Enum.GetName(typeof(Exchange), exchange)} exchange for {nameof(CryptoExOrderBookDistributerFactory)}");
            }
        }
    }
}