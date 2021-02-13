using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Common
{
    public interface IDistributerOptions : IExchange, ICloneable
    {
        int? Frequency { get; }

        int? Limit { get; }
        
        int SilenceLimitInSeconds { get; } 
    }
}