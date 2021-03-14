using System;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using Binance.Net;
using Binance.Net.Interfaces;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class BinancePrivateSocketConnector : BasePrivateSocketConnector, IPrivateSocketConnector
    {
        private readonly IBinanceSocketClient _binanceSocketClient;
        private readonly IConverter _converter;

        public BinancePrivateSocketConnector([NotNull] IPrivateExchangeSettings credentials, IConverter converter)
        {
            Preconditions.CheckNotNull(credentials, converter);
            _binanceSocketClient = new BinanceSocketClient();
            _converter = converter;
            _binanceSocketClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        async Task IPrivateSocketConnector.SubscribeToBalanceUpdatesAsync(CancellationToken ct)
        {
            throw new NotSupportedException();
            /*await _binanceSocketClient.Spot.SubscribeToUserDataUpdatesAsync("balance_sub", account =>
            {
                var notNullableBalances = account.Balances.Where(o => o.Total != 0);
                var balances = _converter.Convert<IEnumerable<BinanceStreamBalance>, IEnumerable<IBalance>>(notNullableBalances);
                OnBalance(balances);
            }, null, null, null);*/
        }

        async Task IPrivateSocketConnector.SubscribeToOrdersUpdatesAsync(string exchangePairName, CancellationToken ct)
        {
            throw new NotSupportedException();
            /*await _binanceSocketClient.Spot.SubscribeToUserDataUpdatesAsync("order_sub", null, binanceOrder =>
            {
                if (binanceOrder.Symbol == exchangePairName)
                {
                    var order = _converter.Convert<BinanceStreamOrderUpdate, IOrder>(binanceOrder);
                    OnOrder(order);
                }
            }, null, null);*/
        }

        public void Dispose()
        {
            _binanceSocketClient?.Dispose();
        }
    }
}