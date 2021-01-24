using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using CryptoExchange.Net.Objects;
using Kraken.Net.Interfaces;
using Kraken.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class KrakenTradingConnector : ITradingConnector
    {
        private readonly IKrakenClient _krakenClient;
        
        public async Task PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            WebCallResult<KrakenPlacedOrder> result =
                await _krakenClient.PlaceOrderAsync(placeOrder.Symbol, OrderSide.Buy, OrderType.Market, placeOrder.Quantity.Value, ct: token);
        }

        public async Task CancelOrderAsync(string symbol, string orderId, CancellationToken token)
        {
           WebCallResult<KrakenCancelResult> result = await _krakenClient.CancelOrderAsync(orderId, ct: token);
        }

        public async Task GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            WebCallResult<OpenOrdersPage> orders = await _krakenClient.GetOpenOrdersAsync(ct:token);
        }
    }
}