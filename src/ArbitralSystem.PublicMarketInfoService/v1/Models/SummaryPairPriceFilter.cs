using System;

namespace ArbitralSystem.PublicMarketInfoService.v1.Models
{
    /// <inheritdoc />
    public class SummaryPairPriceFilter 
    {
        /// <inheritdoc />
        public DateTimeOffset? From { get; set; }
        /// <inheritdoc />
        public DateTimeOffset? To { get; set; }
    }
}