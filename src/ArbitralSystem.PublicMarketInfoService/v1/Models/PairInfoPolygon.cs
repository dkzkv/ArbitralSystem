using System.Collections.Generic;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.PublicMarketInfoService.v1.Models
{
    /// <summary>
    /// Pair info polygon
    /// </summary>
    public class PairInfoPolygon
    {
        /// <summary>
        /// Exchange
        /// </summary>
        public string Exchange { get; set; }
        
        /// <summary>
        /// Distributed pairs in polygon
        /// </summary>
        public IEnumerable<string> UnificatedPairs { get; set; }
    }
}