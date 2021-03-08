using System;
using System.Diagnostics;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models
{
    public class DistributerState 
    {
        public int ClientPairId { get; set; }
        public DateTimeOffset ChangedAt { get; }
        public DistributerSyncStatus PreviousStatus { get; }
        public DistributerSyncStatus CurrentStatus { get; }

        public DistributerState(int clientPairId, DateTimeOffset changedAt, DistributerSyncStatus previousStatus,
            DistributerSyncStatus currentStatus)
        {
            if(clientPairId<=0)
                throw new ArgumentException("Client pair id should be positive value.");
            
            ClientPairId = clientPairId;
            ChangedAt = changedAt;
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
        }
    }
}