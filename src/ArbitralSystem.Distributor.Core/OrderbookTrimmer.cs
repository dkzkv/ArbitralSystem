using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Distributor.Core.Common;

[assembly: InternalsVisibleTo("ArbitralSystem.Distributor.Core.Test")]

namespace ArbitralSystem.Distributor.Core
{
    internal static class OrderbookTrimmer
    {
        public static IOrderBook Trim(IOrderBook orderBook, int count)
        {
            if (count < 1)
                throw new ArgumentException("Cannot trim orderbook, 'count' is not positive value, and greater than '1'.");

            if (orderBook.Asks.Count() <= count && orderBook.Bids.Count() <= count)
                return orderBook;

            var asks = new List<IOrderbookEntry>(orderBook.Asks);
            if (orderBook.Asks.Count() > count)
                asks.RemoveRange(count, asks.Count - count);

            var bids = new List<IOrderbookEntry>(orderBook.Bids);
            if (orderBook.Bids.Count() > count)
                bids.RemoveRange(count, bids.Count - count);

            return new OrderBook(orderBook.Exchange, orderBook.Symbol, orderBook.CatchAt, bids, asks, orderBook.BestBid, orderBook.BestAsk,
                orderBook.ClientPairId);
        }
    }
}