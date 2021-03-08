using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;
[assembly:InternalsVisibleTo("ArbitralSystem.Distributor.Core.Test")]
namespace ArbitralSystem.Distributor.Core.Common
{
    internal class OrderBook : IOrderBook
    {
        public int? ClientPairId { get; }
        public Exchange Exchange { get; }
        public string Symbol { get; }
        public DateTimeOffset CatchAt { get; }
        public IEnumerable<IOrderbookEntry> Bids { get; }
        public IEnumerable<IOrderbookEntry> Asks { get; }
        public IOrderbookEntry BestBid { get; }
        public IOrderbookEntry BestAsk { get; }

        public OrderBook(Exchange exchange,
            string symbol,
            DateTimeOffset catchAt,
            IEnumerable<IOrderbookEntry> bids,
            IEnumerable<IOrderbookEntry> asks,
            IOrderbookEntry bestBid,
            IOrderbookEntry bestAsk,
            int? clientPairId = null)
        {
            Exchange = exchange;
            Symbol = symbol;
            CatchAt = catchAt;
            Bids = bids;
            Asks = asks;
            BestBid = bestBid;
            BestAsk = bestAsk;
            ClientPairId = clientPairId;
        }
    }
}