using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.PrivateConnectors
{
    public interface ITradingConnector : IExchange
    {
        Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token);

        Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token);

        Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token);
        Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token);
    }
}