using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using CryptoExchange.Net.Objects;
using Kucoin.Net.Interfaces;
using Kucoin.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class KucoinTradingConnector : ITradingConnector
    {
        private readonly IKucoinClient _kucoinClient;
        
        public async Task PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            WebCallResult<KucoinNewOrder> orderResult = await _kucoinClient.PlaceOrderAsync(placeOrder.Symbol, KucoinOrderSide.Buy, KucoinNewOrderType.Market, placeOrder.Price, ct: token );
        }

        public async Task CancelOrderAsync(string symbol, string orderId, CancellationToken token)
        {
            WebCallResult<KucoinCancelledOrders> result = await _kucoinClient.CancelOrderAsync(orderId, token);
        }

        public async Task GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            WebCallResult<KucoinPaginated<KucoinOrder>> orders = await _kucoinClient.GetOrdersAsync(symbol, status: KucoinOrderStatus.Active);
        }
    }
}