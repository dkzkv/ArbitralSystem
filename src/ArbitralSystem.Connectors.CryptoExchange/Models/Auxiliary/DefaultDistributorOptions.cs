using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary
{
    internal class DefaultDistributorOptions : IDistributerOptions
    {
        public DefaultDistributorOptions(Exchange exchange)
        {
            Exchange = exchange;
            Frequency = 100;
            Limit = null;
            SilenceLimitInSeconds = 60;
        }
        public Exchange Exchange { get; }
        
        public int? Frequency { get; }
        public int? Limit { get;  }
        public int SilenceLimitInSeconds { get;  }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        
    }
}