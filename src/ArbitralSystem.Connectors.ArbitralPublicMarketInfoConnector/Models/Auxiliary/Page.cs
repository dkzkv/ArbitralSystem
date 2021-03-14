using System.Collections.Generic;
using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models.Auxiliary
{
    internal class Page<T> :IPage<T>
    {
        public int Total { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}