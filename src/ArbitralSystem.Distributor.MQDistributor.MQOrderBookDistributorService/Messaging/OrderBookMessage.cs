using System;
using System.Collections.Generic;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Messaging.Models;
using JetBrains.Annotations;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging
{
    [UsedImplicitly]
    internal class OrderBookMessage : BaseMessage, IOrderBookMessage
    {
        public int ClientPairId { get; set; }
        public DateTimeOffset CatchAt { get; set; }
        public IEnumerable<OrderbookEntry> Bids { get; set; }
        public IEnumerable<OrderbookEntry> Asks { get; set; }
    }
}