using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models
{
    public class OrderBook 
    {
        public int ClientPairId { get; }
        public DateTimeOffset CatchAt { get; }
        public IEnumerable<OrderbookEntry> Bids { get; }
        public IEnumerable<OrderbookEntry> Asks { get; }

        public OrderBook(int clientPairId, DateTimeOffset catchAt, IEnumerable<OrderbookEntry> asks, IEnumerable<OrderbookEntry> bids)
        {
            if(clientPairId<=0)
                throw new ArgumentException("Client pair id should be positive value");
            ClientPairId = clientPairId;
            
            CatchAt = catchAt;
            ValidateOrderBookEntries(bids);
            ValidateOrderBookEntries(asks);
            
            Asks = asks;
            Bids = bids;
        }

        private void ValidateOrderBookEntries(IEnumerable<OrderbookEntry> entries)
        {
            if(entries.GroupBy(o=>o.Price).Any(x=>x.Count() > 1))
                throw new ArgumentException("Price in orderbook entries must be unique!");
        }
    }
}