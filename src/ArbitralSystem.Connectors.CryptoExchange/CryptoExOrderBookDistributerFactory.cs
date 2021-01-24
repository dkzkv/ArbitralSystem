using System;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Distributers;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    public class CryptoExOrderBookDistributerFactory : IOrderBookDistributerFactory
    {
        private readonly IDtoConverter _converter;
        private readonly IDistributerOptions _distributerOptions;
        private readonly ILogger _logger;

        public CryptoExOrderBookDistributerFactory([NotNull] IDistributerOptions distributerOptions,
            [NotNull] IDtoConverter converter,
            ILogger logger)
        {
            _distributerOptions = distributerOptions;
            _converter = converter;
            _logger = logger;
        }

        public IOrderbookDistributor GetInstance(Exchange exchange)
        {
            switch (exchange)
            {
                case Exchange.Binance:
                    return new BinanceOrderbookDistributor(_distributerOptions, _converter, _logger);

                case Exchange.Bittrex:
                    return new BittrexOrderbookDistributor(_distributerOptions, _converter, _logger);

                case Exchange.CoinEx:
                    return new CoinExOrderbookDistributor(_distributerOptions, _converter, _logger);

                case Exchange.Huobi:
                    return new HuobiOrderbookDistributor(_distributerOptions, _converter, _logger);

                case Exchange.Kraken:
                    return new KrakenOrderbookDistributor(_distributerOptions, _converter, _logger);

                case Exchange.Kucoin:
                    return new KucoinOrderbookDistributor(_distributerOptions, _converter, _logger);
                
                case Exchange.Bitmex:
                    return new BitmexOrderbookDistributor(_distributerOptions, _converter, _logger);

                case Exchange.Bitfinex:
                    return new BitfinexOrderbookDistributor(_distributerOptions, _converter, _logger);
                
                default:
                    throw new NotSupportedException(
                        $"Not supported {Enum.GetName(typeof(Exchange), exchange)} exchange for {nameof(CryptoExOrderBookDistributerFactory)}");
            }
        }
    }
}