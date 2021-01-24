using System;
using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models
{
    public interface IArbitralPairInfo : IPairInfo
    {
        DateTimeOffset CreatedAt { get; }

        DateTimeOffset? DelistedAt { get;  }
    }
}