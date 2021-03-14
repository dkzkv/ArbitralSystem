using System.Threading;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    internal struct OrderBookDistributerInstance<T>
    {
        public int? ClientPairId { get; set; }
        public string InstanceSymbol { get; set; }

        public T OrderBook { get; set; }

        public CancellationToken Token { get; set; }
    }
}
