using System.Collections.Generic;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.CoinEx.Models;
using ArbitralSystem.Connectors.Core.Common;

namespace ArbitralSystem.Connectors.CoinEx
{
    public interface ICoinExConnector
    {
        Task<IResponse<IEnumerable<string>>> GetMarketList();

        Task<IResponse<IMarketInfo>> GetMarketSingleInfo(string symbol);
    }
}