using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Domain.MarketInfo;
using Bitfinex.Net;
using Bitfinex.Net.Objects;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.Distributers
{
    internal class BitfinexOrderbookDistributor : BaseOrderBookDistributer<BitfinexSymbolOrderBook>, IOrderbookDistributor
    {
        //1,25,100,250
        private const int DefaultOrderBookLimit = 25;
        private readonly IDistributerOptions _distributerOptions;
        
        public BitfinexOrderbookDistributor([NotNull] IDistributerOptions distributerOptions,
            [NotNull] IConverter converter,
            [NotNull] ILogger logger) : base(distributerOptions, converter, logger)
        {
            _distributerOptions = distributerOptions;
        }

        public override Exchange Exchange => Exchange.Bitfinex;
        protected override BitfinexSymbolOrderBook CreateSymbolOrderBook(string symbol)
        {
            return new BitfinexSymbolOrderBook(symbol,Precision.PrecisionLevel0, _distributerOptions.Limit ?? DefaultOrderBookLimit);
        }
    }
}