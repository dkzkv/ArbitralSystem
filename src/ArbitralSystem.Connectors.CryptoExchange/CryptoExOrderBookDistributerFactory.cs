using System;
using System.Collections.Generic;
using System.Linq;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    public class CryptoExOrderBookDistributerFactory : IOrderBookDistributerFactory
    {
        private readonly IDtoConverter _converter;
        private readonly IDistributerOptions[] _distributerOptions;
        private readonly ILogger _logger;

        public CryptoExOrderBookDistributerFactory([NotNull] IDistributerOptions[] distributerOptions,
            [NotNull] IDtoConverter converter,
            [NotNull] ILogger logger)
        {
            Preconditions.CheckNotNull(distributerOptions, converter, logger);
            if (distributerOptions.GroupBy(o => o.Exchange).Any(o => o.Count() > 1))
                throw new ArgumentException("Ambiguity distributor settings.");
            _distributerOptions = distributerOptions;
            _converter = converter;
            _logger = logger;
        }
        
        public CryptoExOrderBookDistributerFactory([NotNull] IDtoConverter converter,
            [NotNull] ILogger logger)
        {
            Preconditions.CheckNotNull(converter, logger);

            _distributerOptions = Array.Empty<IDistributerOptions>();
            _converter = converter;
            _logger = logger;
        }

        public IOrderbookDistributor GetInstance(Exchange exchange)
        {
            IDistributerOptions options;
            var preSetOptions = _distributerOptions.FirstOrDefault(o => o.Exchange == exchange);
            if (preSetOptions != null)
                options = preSetOptions;
            else
                options = new DefaultDistributorOptions(exchange);
            
            switch (exchange)
            {
                case Exchange.Binance:
                    return new BinanceOrderbookDistributor(options, _converter, _logger);

                case Exchange.Bittrex:
                    return new BittrexOrderbookDistributor(options, _converter, _logger);

                case Exchange.CoinEx:
                    return new CoinExOrderbookDistributor(options, _converter, _logger);

                case Exchange.Huobi:
                    return new HuobiOrderbookDistributor(options, _converter, _logger);

                case Exchange.Kraken:
                    return new KrakenOrderbookDistributor(options, _converter, _logger);

                case Exchange.Kucoin:
                    return new KucoinOrderbookDistributor(options, _converter, _logger);
                
                case Exchange.Bitmex:
                    return new BitmexOrderbookDistributor(options, _converter, _logger);

                case Exchange.Bitfinex:
                    return new BitfinexOrderbookDistributor(options, _converter, _logger);
                
                default:
                    throw new NotSupportedException(
                        $"Not supported {Enum.GetName(typeof(Exchange), exchange)} exchange for {nameof(CryptoExOrderBookDistributerFactory)}");
            }
        }
    }
}