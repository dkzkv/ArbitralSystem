using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Domain.MarketInfo;
using Bitmex.Net.Client;

namespace ArbitralSystem.Connectors.CryptoExchange.Distributers
{
    internal class BitmexOrderbookDistributor : BaseOrderBookDistributer<BitmexSymbolOrderBook>, IOrderbookDistributor
    {
        private const int DefaultOrderBookLimit = 20;
        private readonly IDistributerOptions _distributerOptions;


        public BitmexOrderbookDistributor(IDistributerOptions distributerOptions,
            IConverter converter,
            ILogger logger)
            : base(distributerOptions, converter, logger)
        {
            _distributerOptions = distributerOptions;
        }

        public override Exchange Exchange => Exchange.Bitmex;
        protected override BitmexSymbolOrderBook CreateSymbolOrderBook(string symbol)
        {
            return new BitmexSymbolOrderBook(symbol, new BitmexSocketOrderBookOptions(symbol, tickSize: 100));
        }
    }
}