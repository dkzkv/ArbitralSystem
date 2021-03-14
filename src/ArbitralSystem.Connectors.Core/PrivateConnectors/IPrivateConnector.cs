using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.PrivateConnectors
{
    public interface IPrivateConnector : IExchange
    {
        Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token = default);

        Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token = default);

        Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token = default);
        Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token = default);
        Task<IEnumerable<IBalance>> GetBalanceAsync(CancellationToken token = default);
    }
}