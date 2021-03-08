using System;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Distributers
{
    public class OrderBookPairInfo
    {
        public int? ClientPairId { get; }
        public string UnificatedPairName { get; }
        public string ExchangePairName { get; }
        public Exchange Exchange { get; }

        public OrderBookPairInfo(Exchange exchange,string exchangePairName,string unificatedPairName , int? clientPairId = null)
        {
            if(clientPairId.HasValue && clientPairId.Value <= 0)
                throw new ArgumentException("Client order id should be positive value.");
            ClientPairId = clientPairId;
            
            if (exchange == Exchange.Undefined)
                throw new WebsocketException("Un expected exchange to start orderbook distribution");
            
            if(string.IsNullOrEmpty(exchangePairName) || string.IsNullOrEmpty(exchangePairName))
                throw new WebsocketException("Pair info can't be empty to start orderbook distribution");

            Exchange = exchange;
            ExchangePairName = exchangePairName;
            UnificatedPairName = unificatedPairName;
        }
    }
}