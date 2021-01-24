using ArbitralSystem.Connectors.Core.Types;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Distributers
{
    public interface IOrderbookDistributor
    {
        Exchange Exchange { get; }

        IDistributerState DistributerState { get; }

        Task<Task> StartDistributionAsync(OrderBookPairInfo pairInfo, CancellationToken token);

        event OrderBookDelegate OrderBookChanged;

        event DistributerStateDelegate DistributerStateChanged;
    }
}