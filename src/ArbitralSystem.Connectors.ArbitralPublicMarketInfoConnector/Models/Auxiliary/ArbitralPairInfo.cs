using System;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models.Auxiliary
{
    [UsedImplicitly]
    internal class ArbitralPairInfo : IArbitralPairInfo
    {
        public Exchange Exchange { get;  set; }
        public string UnificatedPairName { get; set; }
        public string ExchangePairName { get;set; }
        public string BaseCurrency { get; set;}
        public string QuoteCurrency { get; set;}
        
        public int AmountPrecision { get; set; }
        public decimal? MinMarketOrderValue { get; set; }
        public decimal? MaxMarketOrderValue { get; set; }
        public decimal? MinLimitOrderValue { get; set; }
        public decimal? MaxLimitOrderValue { get; set; }
        public decimal? MinMarketOrderAmount { get; set; }
        public decimal? MaxMarketOrderAmount { get; set; }
        public decimal? MinLimitOrderAmount { get; set; }
        public decimal? MaxLimitOrderAmount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DelistedAt { get; set; }
    }
}