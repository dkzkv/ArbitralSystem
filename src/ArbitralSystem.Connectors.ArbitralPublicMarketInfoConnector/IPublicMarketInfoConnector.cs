using System.Collections.Generic;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models;
using ArbitralSystem.Connectors.Core.Common;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector
{
    public interface IPublicMarketInfoConnector
    {
        Task<IResponse<IEnumerable<IArbitralPairInfo>>> GetPairs(Exchange exchange);
        
        Task<IResponse<IPage<IArbitralPairInfo>>> GetPairs(ArbitralPairInfoFilterFilter filter);
    }
}