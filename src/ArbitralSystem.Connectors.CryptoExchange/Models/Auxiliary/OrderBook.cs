using System.Collections.Generic;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models
{
    internal class OrderBook : IOrderBook
    {
        public string Symbol { get; set; }
        public IEnumerable<IOrderbookEntry> Bids { get; set; }
        public IEnumerable<IOrderbookEntry> Asks { get;set;  }
        public IOrderbookEntry BestBid { get; set; }
        public IOrderbookEntry BestAsk { get; set; }
        public Exchange Exchange { get; set; }
    }
}