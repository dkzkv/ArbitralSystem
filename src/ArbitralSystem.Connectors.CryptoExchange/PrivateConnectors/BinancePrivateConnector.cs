using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.SpotData;
using Binance.Net.Objects.Spot.WalletData;
using CryptoExchange.Net.ExchangeInterfaces;
using CryptoExchange.Net.Objects;
using OrderSide = Binance.Net.Enums.OrderSide;
using OrderType = Binance.Net.Enums.OrderType;
using Arbitral = ArbitralSystem.Domain.MarketInfo;

[assembly: InternalsVisibleTo("ArbitralSystem.Connectors.Integration.Test")]

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    internal class BinanceTestPrivateConnector : BinancePrivateConnector, IPrivateConnector
    {
        private readonly IBinanceClient _binanceClient;
        private readonly IConverter _converter;

        public BinanceTestPrivateConnector(ICredentials credentials, IConverter converter, IBinanceClient binanceClient = null) : base(credentials, converter,
            binanceClient)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (binanceClient == null)
                _binanceClient = new BinanceClient();
            else
                _binanceClient = binanceClient;

            _converter = converter;
            _binanceClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        async Task<string> IPrivateConnector.PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            var placedOrderResult = await _binanceClient.Spot.Order.PlaceTestOrderAsync(placeOrder.ExchangePairName,
                side: _converter.Convert<Arbitral.OrderSide, OrderSide>(placeOrder.OrderSide),
                type: _converter.Convert<Arbitral.OrderType, OrderType>(placeOrder.OrderType),
                quantity: placeOrder.Quantity,
                price: placeOrder.Price,
                timeInForce: placeOrder.OrderType == Arbitral.OrderType.Limit ? TimeInForce.GoodTillCancel : (TimeInForce?)null,
                newClientOrderId: placeOrder.ClientOrderId,
                ct: token);

            ValidateResponse(placedOrderResult);
            return placedOrderResult.Data.OrderId.ToString();
        }
    }

    internal class BinancePrivateConnector : ExchangeBaseConnector, IPrivateConnector
    {
        private readonly IBinanceClient _binanceClient;
        private readonly IConverter _converter;

        public BinancePrivateConnector(ICredentials credentials, IConverter converter, IBinanceClient binanceClient = null)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (binanceClient == null)
                _binanceClient = new BinanceClient();
            else
                _binanceClient = binanceClient;

            _converter = converter;
            _binanceClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        public override Exchange Exchange => Exchange.Binance;

        async Task<string> IPrivateConnector.PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            var placedOrderResult = await _binanceClient.Spot.Order.PlaceOrderAsync(placeOrder.ExchangePairName,
                side: _converter.Convert<Arbitral.OrderSide, OrderSide>(placeOrder.OrderSide),
                type: _converter.Convert<Arbitral.OrderType, OrderType>(placeOrder.OrderType),
                quantity: placeOrder.Quantity,
                price: placeOrder.Price,
                timeInForce: placeOrder.OrderType == Arbitral.OrderType.Limit ? TimeInForce.GoodTillCancel : (TimeInForce?)null,
                newClientOrderId: placeOrder.ClientOrderId,
                ct: token);

            ValidateResponse(placedOrderResult);
            return placedOrderResult.Data.OrderId.ToString();
        }

        async Task IPrivateConnector.CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token)
        {
            ValidateExchangeArgument(cancelOrder);
            if (!string.IsNullOrEmpty(cancelOrder.OrderId) && Int64.TryParse(cancelOrder.OrderId, out var orderId))
            {
                var result = await _binanceClient.Spot.Order.CancelOrderAsync(cancelOrder.Symbol, orderId, ct: token);
                ValidateResponse(result);
            }
            else if (!string.IsNullOrEmpty(cancelOrder.ClientOrderId))
            {
                var result = await _binanceClient.Spot.Order.CancelOrderAsync(cancelOrder.Symbol, origClientOrderId: cancelOrder.ClientOrderId, ct: token);
                ValidateResponse(result);
            }
            else
                throw new ArgumentException("Cancel order id or client order id is not valid");
        }

        async Task<IEnumerable<IOrder>> IPrivateConnector.GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            if (string.IsNullOrEmpty(symbol))
                throw new ArgumentException("Symbol should not be empty");

            var openOrdersAsync = await _binanceClient.Spot.Order.GetOpenOrdersAsync(symbol, ct: token);
            ValidateResponse(openOrdersAsync);
            return _converter.Convert<IEnumerable<BinanceOrder>, IEnumerable<IOrder>>(openOrdersAsync.Data);
        }

        async Task<IOrder> IPrivateConnector.GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            WebCallResult<BinanceOrder> orderResult = null;
            if (!string.IsNullOrEmpty(orderRequest.OrderId) && Int64.TryParse(orderRequest.OrderId, out var orderId))
            {
                orderResult = await _binanceClient.Spot.Order.GetOrderAsync(orderRequest.Symbol, orderId, ct: token);
            }
            else if (!string.IsNullOrEmpty(orderRequest.ClientOrderId))
            {
                orderResult = await _binanceClient.Spot.Order.GetOrderAsync(orderRequest.Symbol, origClientOrderId: orderRequest.ClientOrderId, ct: token);
            }
            else
            {
                throw new ArgumentException($"Order search argument is not valid for Exchange: {Exchange}");
            }

            ValidateResponse(orderResult);
            return _converter.Convert<BinanceOrder, IOrder>(orderResult.Data);
        }

        async Task<IEnumerable<IBalance>> IPrivateConnector.GetBalanceAsync(CancellationToken token)
        {
            var accountInfoResult = await _binanceClient.General.GetAccountInfoAsync(ct: token);
            ValidateResponse(accountInfoResult);
            var notNullableBalances = accountInfoResult.Data.Balances.Where(o => o.Total != 0);
            return _converter.Convert<IEnumerable<BinanceBalance>, IEnumerable<IBalance>>(notNullableBalances);
        }
    }
}