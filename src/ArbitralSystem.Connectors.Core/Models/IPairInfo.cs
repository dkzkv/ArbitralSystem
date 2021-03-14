using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models
{
    public interface IPairInfo : IExchange
    {
        string UnificatedPairName { get; }

        string ExchangePairName { get; }

        string BaseCurrency { get; }

        string QuoteCurrency { get; }
        
        
        int AmountPrecision { get; }
        
        //value is in quote currency 
        decimal? MinMarketOrderValue { get; }
        decimal? MaxMarketOrderValue { get; }
        
        decimal? MinLimitOrderValue { get; }
        decimal? MaxLimitOrderValue { get; }
        
        //amount is in base currency
        decimal? MinMarketOrderAmount { get; }
        decimal? MaxMarketOrderAmount { get; }
        
        decimal? MinLimitOrderAmount { get; }
        decimal? MaxLimitOrderAmount { get; }
        
    }
}