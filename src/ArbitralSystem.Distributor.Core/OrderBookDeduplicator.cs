using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Distributor.Core.Common;
using ArbitralSystem.Distributor.Core.Interfaces;

namespace ArbitralSystem.Distributor.Core
{
    public class LockedOrderBookDeDuplicator : IOrderBookDeDuplicator
    {
        private Dictionary<DistributorOrderBook, DistributorOrderBook> _previousOrderBooks;

        public LockedOrderBookDeDuplicator()
        {
            _previousOrderBooks = new Dictionary<DistributorOrderBook, DistributorOrderBook>();
        }

        private static object _lockObj = new object();

        public bool IsDuplicate(IDistributorOrderBook distributorOrderBook)
        {
            var orderBook = new DistributorOrderBook(distributorOrderBook.Exchange,
                distributorOrderBook.Symbol,
                distributorOrderBook.CatchAt,
                distributorOrderBook.Bids,
                distributorOrderBook.Asks,
                distributorOrderBook.BestBid,
                distributorOrderBook.BestAsk,
                distributorOrderBook.ClientPairId);

            lock (_lockObj)
            {
                if (!_previousOrderBooks.ContainsKey(orderBook))
                {
                    _previousOrderBooks.Add(orderBook, orderBook);
                    return false;
                }

                if (_previousOrderBooks.TryGetValue(orderBook, out var innerOrderBook))
                {
                    if (innerOrderBook != null && !(innerOrderBook.Equals(orderBook)))
                    {
                        _previousOrderBooks[orderBook] = innerOrderBook;
                        return false;
                    }
                    return true;
                }

                throw new Exception("Unexpected exception in locked deduplicator, value not exsited");
            }
        }
    }

    public class ConcurrentOrderBookDeDuplicator : IOrderBookDeDuplicator
    {
        private ConcurrentDictionary<int, DistributorOrderBook> _previousOrderBooks;

        public ConcurrentOrderBookDeDuplicator()
        {
            _previousOrderBooks = new ConcurrentDictionary<int, DistributorOrderBook>();
        }

        public bool IsDuplicate(IDistributorOrderBook distributorOrderBook)
        {
            var orderBook = new DistributorOrderBook(distributorOrderBook.Exchange,
                distributorOrderBook.Symbol,
                distributorOrderBook.CatchAt,
                distributorOrderBook.Bids,
                distributorOrderBook.Asks,
                distributorOrderBook.BestBid,
                distributorOrderBook.BestAsk,
                distributorOrderBook.ClientPairId);

            if (!_previousOrderBooks.ContainsKey(orderBook.GetHashCode()))
            {
                _previousOrderBooks.AddOrUpdate(orderBook.GetHashCode(), orderBook, (key, oldVal) => orderBook);
                return false;
            }
            else
            {
                if (_previousOrderBooks.TryGetValue(orderBook.GetHashCode(), out var innerOrderBook))
                {
                    if (innerOrderBook != null && !(innerOrderBook.Equals(orderBook)))
                    {
                        _previousOrderBooks.TryUpdate(orderBook.GetHashCode(), orderBook, orderBook);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}