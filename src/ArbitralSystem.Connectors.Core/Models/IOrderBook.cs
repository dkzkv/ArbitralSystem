using System.Collections.Generic;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models
{
    public interface IOrderBook : IExchange
    {
        string Symbol { get;  }
        IEnumerable<IOrderbookEntry> Bids { get;  }
        IEnumerable<IOrderbookEntry> Asks { get;  }
        IOrderbookEntry BestBid { get;  }
        IOrderbookEntry BestAsk { get;  }
    }
}