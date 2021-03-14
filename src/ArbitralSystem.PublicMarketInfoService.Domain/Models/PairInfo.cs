using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.PublicMarketInfoService.Domain.Models
{
    public class PairInfo 
    {
        public int? Id { get; }
        public string ExchangePairName { get; }
        public string UnificatedPairName { get; }
        public string BaseCurrency { get; }
        public string QuoteCurrency { get; }
        public DateTimeOffset CreatedAt { get; }
        public DateTimeOffset? DelistedAt { get; private set; }
        public Exchange Exchange { get; private set; }

        public PairInfo(string exchangePairName, string baseCurrency, string quoteCurrency, string unificatedPairName, Exchange exchange)
        {
            ValidatePair(baseCurrency, quoteCurrency, unificatedPairName);
            Id = null;
            //can be empty
            ExchangePairName = exchangePairName;
            UnificatedPairName = unificatedPairName;
            BaseCurrency = baseCurrency;
            QuoteCurrency = quoteCurrency;
            SetExchange(exchange);
            CreatedAt = DateTimeOffset.UtcNow;
            DelistedAt = null;
        }
        
        /// <summary>
        /// Just for mapping from persistence layer
        /// </summary>
        public PairInfo(int id,
            string exchangePairName,
            string unificatedPairName,
            string baseCurrency,
            string quoteCurrency,
            DateTimeOffset createdAt,
            DateTimeOffset? delistedAt,
            Exchange exchange)
        {
            Id = id;
            ExchangePairName = exchangePairName;
            UnificatedPairName = unificatedPairName;
            BaseCurrency = baseCurrency;
            
            QuoteCurrency = quoteCurrency;
            CreatedAt = createdAt;
            DelistedAt = delistedAt;
            Exchange = exchange;
        }

        public void SetPairAsDelisted()
        {
            DelistedAt = DateTimeOffset.UtcNow;
        }

        private void ValidatePair(string baseCurrency, string quoteCurrency, string unificatedPairName)
        {
            if (string.IsNullOrEmpty(baseCurrency) ||
                string.IsNullOrEmpty(quoteCurrency) ||
                string.IsNullOrEmpty(unificatedPairName))
                throw new ArgumentException("Pair information can not be empty");
        }

        private void SetExchange(Exchange exchange)
        {
            if (exchange == Exchange.Undefined)
                throw new ArgumentException("Exchange can not be undefined");

            Exchange = exchange;
        }
    }
}