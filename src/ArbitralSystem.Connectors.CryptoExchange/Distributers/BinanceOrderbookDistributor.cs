using System.Runtime.CompilerServices;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Domain.MarketInfo;
using Binance.Net.Objects.Spot;
using Binance.Net.SymbolOrderBooks;

[assembly: InternalsVisibleTo("ArbitralSystem.Connectors.Integration.Test")]
namespace ArbitralSystem.Connectors.CryptoExchange.Distributers
{
    internal class BinanceOrderbookDistributor : BaseOrderBookDistributer<BinanceSpotSymbolOrderBook>, IOrderbookDistributor
    {
        private const int DefaultOrderBookLimit = 20;
        private readonly IDistributerOptions _distributerOptions;

        public BinanceOrderbookDistributor(IDistributerOptions distributerOptions,
            IConverter converter,
            ILogger logger)
            : base(distributerOptions, converter, logger)
        {
            _distributerOptions = distributerOptions;
        }

        public override Exchange Exchange => Exchange.Binance;

        protected override BinanceSpotSymbolOrderBook CreateSymbolOrderBook(string symbol)
        {
            return new BinanceSpotSymbolOrderBook(symbol,
                new BinanceOrderBookOptions(_distributerOptions.Limit ?? DefaultOrderBookLimit));
        }
    }
}