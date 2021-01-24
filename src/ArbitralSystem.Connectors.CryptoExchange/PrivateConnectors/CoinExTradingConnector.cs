using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using CoinEx.Net.Interfaces;
using CoinEx.Net.Objects;
using CryptoExchange.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class CoinExTradingConnector
    {
        private readonly ICoinExClient _coinExClient;


        public async Task PlaceOrderAsync(IOrder order)
        {
            WebCallResult<CoinExOrder> orderResult = _coinExClient.PlaceMarketOrder(order.Symbol, TransactionType.Buy, 10);
        }

        public async Task CancelOrderAsync(string symbol, string orderId)
        {
            var canceledOrder = await _coinExClient.CancelOrderAsync(symbol, Int64.Parse(orderId));
        }

        public async Task GetOpenOrderAsync(string symbol)
        {
            WebCallResult<CoinExPagedResult<CoinExOrder>> openedOrders = await _coinExClient.GetOpenOrdersAsync(symbol, 0, 100);
        }
        
    }
    
    public class CoinExWalletConnector
    {
        private readonly ICoinExClient _coinExClient;


        public async Task AvailableAssets(string symbol, CancellationToken ct)
        {
            WebCallResult<Dictionary<string, CoinExBalance>> balances = _coinExClient.GetBalances(ct: ct);
        }
    }
    
}