using System.Collections.Generic;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Messaging.Models;
using JetBrains.Annotations;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging
{
    [UsedImplicitly]
    internal class HeartBeatOrderBookDistributorMessage : BaseMessage, IHeartBeatOrderBookDistributorMessage
    {
        public HeartBeatOrderBookDistributorMessage(IEnumerable<HeartBeatOrderBookDistributor> heartBeatBatch)
        {
            HeartBeatBatch = heartBeatBatch;
        }
        
        public IEnumerable<HeartBeatOrderBookDistributor> HeartBeatBatch { get; }
    }
}