using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.PublicMarketInfoService.Domain.Queries.QueryModels
{

    public interface IPairInfo 
    {
        int Id { get; }
        string ExchangePairName { get; }
        string UnificatedPairName { get; }
        string BaseCurrency { get; }
        string QuoteCurrency { get; }
        DateTimeOffset CreatedAt { get; }
        DateTimeOffset? DelistedAt { get; }
        Exchange Exchange { get; }
    }
}