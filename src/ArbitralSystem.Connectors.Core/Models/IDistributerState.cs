using System;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models
{
    public interface IDistributerState
    {
        int? ClientPairId { get; }
        string Symbol { get;}
        Exchange Exchange { get;}
        DateTimeOffset ChangedAt { get; }
        DistributerSyncStatus PreviousStatus { get; }
        DistributerSyncStatus CurrentStatus { get; }
    }
}