using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Distributor.Core.Interfaces
{
    public interface IOrderBookDeDuplicator
    {
        bool IsDuplicate(IDistributorOrderBook distributorOrderBook);
    }
}