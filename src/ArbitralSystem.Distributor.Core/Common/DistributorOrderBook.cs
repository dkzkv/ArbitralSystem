using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;
[assembly:InternalsVisibleTo("ArbitralSystem.Distributor.Core.Test")]
namespace ArbitralSystem.Distributor.Core.Common
{
    internal class DistributorOrderBook : IDistributorOrderBook , IEquatable<DistributorOrderBook>
    {
        public int? ClientPairId { get; }
        public Exchange Exchange { get; }
        public string Symbol { get; }
        public DateTimeOffset CatchAt { get; }
        public IEnumerable<IOrderbookEntry> Bids { get; }
        public IEnumerable<IOrderbookEntry> Asks { get; }
        public IOrderbookEntry BestBid { get; }
        public IOrderbookEntry BestAsk { get; }

        public DistributorOrderBook(Exchange exchange,
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

        public bool Equals(DistributorOrderBook other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            
            return ClientPairId == other.ClientPairId &&
                   Exchange == other.Exchange &&
                   Symbol == other.Symbol &&
                   IsEntryEquals(Bids.ToArray(), other.Bids.ToArray()) &&
                   IsEntryEquals(Asks.ToArray(), other.Asks.ToArray()) &&
                   ((BestBid.Price == other.BestBid.Price) && (BestBid.Quantity == other.BestBid.Quantity)) &&
                   ((BestAsk.Price == other.BestAsk.Price) && (BestAsk.Quantity == other.BestAsk.Quantity));

            bool IsEntryEquals(IOrderbookEntry[] entries, IOrderbookEntry[] otherEntries)
            {
                if (entries.Count() != otherEntries.Count())
                   return false;

                bool isEqual = true;
                for (int i = 0; i < entries.Count() ; i++)
                {
                    if((entries[i].Price !=  otherEntries[i].Price) || (entries[i].Quantity !=  otherEntries[i].Quantity))
                    {
                        isEqual = false;
                        break;
                    }
                }
                return isEqual;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DistributorOrderBook) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientPairId, (int) Exchange, Symbol);
        }
    }
}