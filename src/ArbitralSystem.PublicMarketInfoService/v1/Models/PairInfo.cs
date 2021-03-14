using System;
using System.Collections.Generic;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.PublicMarketInfoService.v1.Models
{
    /// <summary>
    /// Pair info
    /// </summary>
    public class PairInfo
    {
        /// <summary>
        /// Pair info system id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Exchange pair name.
        /// </summary>
        public string ExchangePairName { get; set; }

        /// <summary>
        /// Unificated pair name name {BASE/QUOTE}.
        /// </summary>
        public string UnificatedPairName { get; set; }

        /// <summary>
        /// Base currency.
        /// </summary>
        public string BaseCurrency { get; set; }

        /// <summary>
        /// Quote currency.
        /// </summary>
        public string QuoteCurrency { get; set; }

        /// <summary>
        /// In what time pair was received in service
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// In what time pair was delisted in service
        /// </summary>
        public DateTimeOffset? DelistedAt { get; set; }

        /// <summary>
        /// Exchange name
        /// </summary>
        public Exchange Exchange { get; set; }
    }
}