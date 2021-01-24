using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using CryptoExchange.Net.Objects;
using Huobi.Net.Interfaces;
using Huobi.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class HuobiTradingConnector : ITradingConnector
    {
        private readonly IHuobiClient _huobiClient;

        public async Task PlaceOrderAsync(IOrder order, CancellationToken ct)
        {
            long accountId = 1;
            WebCallResult<long> placedOrder = await _huobiClient.PlaceOrderAsync(accountId, order.Symbol, HuobiOrderType.MarketBuy, order.Quantity.Value, ct: ct);
        }

        public async Task CancelOrderAsync(string symbol, string orderId, CancellationToken token)
        {
            await _huobiClient.CancelOrderAsync(Int64.Parse(orderId), ct: token);
        }

        public async Task GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
           var openorders = await _huobiClient.GetOpenOrdersAsync(symbol: symbol);
        }
    }
}