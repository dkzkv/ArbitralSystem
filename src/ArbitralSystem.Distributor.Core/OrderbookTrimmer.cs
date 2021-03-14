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
        public static IDistributorOrderBook Trim(IDistributorOrderBook distributorOrderBook, int count)
        {
            if (count < 1)
                throw new ArgumentException("Cannot trim orderbook, 'count' is not positive value, and greater than '1'.");

            if (distributorOrderBook.Asks.Count() <= count && distributorOrderBook.Bids.Count() <= count)
                return distributorOrderBook;

            var asks = new List<IOrderbookEntry>(distributorOrderBook.Asks);
            if (distributorOrderBook.Asks.Count() > count)
                asks.RemoveRange(count, asks.Count - count);

            var bids = new List<IOrderbookEntry>(distributorOrderBook.Bids);
            if (distributorOrderBook.Bids.Count() > count)
                bids.RemoveRange(count, bids.Count - count);

            return new DistributorOrderBook(distributorOrderBook.Exchange, distributorOrderBook.Symbol, distributorOrderBook.CatchAt, bids, asks, distributorOrderBook.BestBid, distributorOrderBook.BestAsk,
                distributorOrderBook.ClientPairId);
        }
    }
}