using System.Collections.Generic;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Bitfinex.Models;
using ArbitralSystem.Connectors.Core.Common;

namespace ArbitralSystem.Connectors.Bitfinex
{
    public interface IBitfinexConnector
    {
        Task<IResponse<IEnumerable<IReduction>>> GetCurrencyReductionsAsync();
    }
}