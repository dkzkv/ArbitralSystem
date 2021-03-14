using System;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models
{
    internal class DistributerState : IDistributerState
    {
        public int? ClientPairId { get; set; }
        public string Symbol { get; set;}
        public Exchange Exchange { get; set;}
        public DateTimeOffset ChangedAt { get; set; }
        public DistributerSyncStatus PreviousStatus { get; set; }
        public DistributerSyncStatus CurrentStatus { get; set;}
    }
}