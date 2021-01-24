using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.PrivateConnectors
{
    public interface ITradingConnector : IExchange
    {
        Task PlaceOrderAsync(IOrder order, CancellationToken token);

        Task CancelOrderAsync(string symbol, string orderId, CancellationToken token);

        Task GetOpenOrdersAsync(string symbol, CancellationToken token);
    }
}