using System;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.PublicMarketInfoService.Domain.Models;
using ArbitralSystem.PublicMarketInfoService.Domain.Queries.QueryModels;
using JetBrains.Annotations;

namespace ArbitralSystem.PublicMarketInfoService.Persistence.Mapping.AuxiliaryModels
{
    [UsedImplicitly]
    internal class PairInfoAuxiliaryModel : IPairInfo
    {
        public int Id { get; set; }
        public string ExchangePairName { get; set; }
        public string UnificatedPairName { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set;}
        public DateTimeOffset CreatedAt { get; set;}
        public DateTimeOffset? DelistedAt { get; set;}
        public Exchange Exchange { get; set;}
    }
}