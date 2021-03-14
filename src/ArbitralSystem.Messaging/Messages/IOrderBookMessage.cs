using System;
using System.Collections.Generic;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Messaging.Models;

namespace ArbitralSystem.Messaging.Messages
{
    public interface IOrderBookMessage : ICorrelation
    {
        int ClientPairId { get; set; }
        DateTimeOffset CatchAt { get; }
        IEnumerable<OrderbookEntry> Bids { get; }
        IEnumerable<OrderbookEntry> Asks { get; }
    }
}