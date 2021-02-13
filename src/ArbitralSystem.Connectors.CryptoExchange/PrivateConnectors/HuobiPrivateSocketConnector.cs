using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.Core.Types;
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.UserStream;
using CryptoExchange.Net.Authentication;
using Huobi.Net;
using Huobi.Net.Interfaces;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public abstract class BasePrivateSocketConnector
    {
        private event BalanceDelegate BalanceHandler;

        public event BalanceDelegate BalanceChanged
        {
            add => BalanceHandler += value;
            remove => BalanceHandler -= value;
        }

        protected void OnBalance(IEnumerable<IBalance> balance) => BalanceHandler?.Invoke(balance);

        public event OrderDelegate OrderHandler;

        public event OrderDelegate OrderChanged
        {
            add => OrderHandler += value;
            remove => OrderHandler -= value;
        }

        protected void OnOrder(IOrder order) => OrderHandler?.Invoke(order);
    }

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
            await _binanceSocketClient.Spot.SubscribeToUserDataUpdatesAsync("balance_sub", account =>
            {
                var notNullableBalances = account.Balances.Where(o => o.Total != 0);
                var balances = _converter.Convert<IEnumerable<BinanceStreamBalance>, IEnumerable<IBalance>>(notNullableBalances);
                OnBalance(balances);
            }, null, null, null, null);
        }

        async Task IPrivateSocketConnector.SubscribeToOrdersUpdatesAsync(string exchangePairName, CancellationToken ct)
        {
            await _binanceSocketClient.Spot.SubscribeToUserDataUpdatesAsync("order_sub", null, binanceOrder =>
            {
                if (binanceOrder.Symbol == exchangePairName)
                {
                    var order = _converter.Convert<BinanceStreamOrderUpdate, IOrder>(binanceOrder);
                    OnOrder(order);
                }
            }, null, null, null);
        }

        public void Dispose()
        {
            _binanceSocketClient?.Dispose();
        }
    }

    public class HuobiPrivateSocketConnector : BasePrivateSocketConnector, IPrivateSocketConnector
    {
        private readonly IHuobiSocketClient _huobiSocketClient;
        private readonly IConverter _converter;
        private readonly long _accountId;
        
        public HuobiPrivateSocketConnector([NotNull] IPrivateExchangeSettings credentials, IConverter converter)
        {
            Preconditions.CheckNotNull(credentials, converter);
            _huobiSocketClient = new HuobiSocketClient(new HuobiSocketClientOptions
                {ApiCredentials = new ApiCredentials(credentials.ApiKey, credentials.SecretKey)});
            _accountId = credentials.AccountId ?? throw new NullReferenceException(nameof(credentials.AccountId));
            _converter = converter;
        }


        public async Task SubscribeToBalanceUpdatesAsync(CancellationToken ct = default)
        {
            
            
            await _huobiSocketClient.SubscribeToAccountUpdatesAsync(update =>
            {
                //update.
            });
        }

        public async Task SubscribeToOrdersUpdatesAsync(string exchangePairName, CancellationToken ct = default)
        {
            await _huobiSocketClient.SubscribeToOrderDetailsUpdatesAsync(exchangePairName, update =>
            {
                
            });
        }


        public void Dispose()
        {
            _huobiSocketClient?.Dispose();
        }
    }
}