using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models;


namespace ArbitralSystem.Connectors.Core.PublicConnectors
{
    public interface IPublicConnector
    {
        Task<long> GetServerTime(CancellationToken ct = default (CancellationToken));
        Task<IEnumerable<IPairInfo>> GetPairsInfo(CancellationToken ct = default (CancellationToken));
        Task<IEnumerable<IPairPrice>> GetPairPrices(CancellationToken ct = default (CancellationToken));
        Task<IOrderBook> GetOrderBook(string symbol, CancellationToken ct = default (CancellationToken));
    }
}
