using System;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Messaging.Messages;
using JetBrains.Annotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging
{
    [UsedImplicitly]
    internal class DistributerStateMessage : BaseMessage, IDistributerStateMessage
    {
        public int ClientPairId { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public DistributerSyncStatus PreviousStatus { get; set;}
        public DistributerSyncStatus CurrentStatus { get; set;}
    }
}