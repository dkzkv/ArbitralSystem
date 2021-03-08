using System;
using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models
{
    public interface IArbitralPairInfo : IPairInfo
    {
        public int PairId { get; }
        DateTimeOffset CreatedAt { get; }

        DateTimeOffset? DelistedAt { get;  }
    }
}