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
using ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using CryptoExchange.Net.Objects;
using Huobi.Net;
using Huobi.Net.Enums;
using Huobi.Net.Interfaces;
using Huobi.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    internal class HuobiPrivateConnector : ExchangeBaseConnector, IPrivateConnector
    {
        private readonly IHuobiClient _huobiClient;
        private readonly IConverter _converter;
        private readonly long _accountId;

        public HuobiPrivateConnector(long accountId, ICredentials credentials, IConverter converter, IHuobiClient huobiClient = null)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (_huobiClient == null)
                _huobiClient = new HuobiClient();
            else
                _huobiClient = huobiClient;

            _accountId = accountId;
            _converter = converter;
            _huobiClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        public override Exchange Exchange => Exchange.Huobi;

        async Task<string> IPrivateConnector.PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            HuobiOrderType huobiOrderType;
            if (placeOrder.OrderType == OrderType.Market)
                huobiOrderType = placeOrder.OrderSide == OrderSide.Buy ? HuobiOrderType.MarketBuy : HuobiOrderType.MarketSell;
            else if (placeOrder.OrderType == OrderType.Limit)
                huobiOrderType = placeOrder.OrderSide == OrderSide.Buy ? HuobiOrderType.LimitBuy : HuobiOrderType.LimitSell;
            else
                throw new NotSupportedException($"Not supported order type: {placeOrder.OrderType} for Exchange: {Exchange}");

            WebCallResult<long> placedOrderResult;

            switch (placeOrder.OrderType)
            {
                case OrderType.Limit:
                    placedOrderResult = await _huobiClient.PlaceOrderAsync(_accountId,
                        placeOrder.ExchangePairName,
                        huobiOrderType,
                        placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for limit order should not be null, for Exchange: {Exchange}"),
                        price: placeOrder.Price ?? throw new ArgumentNullException($"Price for limit order should not be null, for Exchange: {Exchange}"),
                        clientOrderId: placeOrder.ClientOrderId,
                        ct: token);
                    break;
                case OrderType.Market:
                    placedOrderResult = await _huobiClient.PlaceOrderAsync(_accountId,
                        placeOrder.ExchangePairName,
                        huobiOrderType,
                        placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for limit order should not be null, for Exchange: {Exchange}"),
                        clientOrderId: placeOrder.ClientOrderId,
                        source: SourceType.Spot,
                        ct: token);
                    break;
                default:
                    throw new ArgumentException($"Invalid order type {placeOrder.OrderType}, for Exchange: {Exchange}");
            }
            
            ValidateResponse(placedOrderResult);
            return placedOrderResult.Data.ToString();
        }


        public async Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token)
        {
            ValidateExchangeArgument(cancelOrder);
            if (!string.IsNullOrEmpty(cancelOrder.OrderId) && Int64.TryParse(cancelOrder.OrderId, out var orderId))
            {
                var result = await _huobiClient.CancelOrderAsync(orderId, ct: token);
                ValidateResponse(result);
            }
            else if (!string.IsNullOrEmpty(cancelOrder.ClientOrderId))
            {
                var result = await _huobiClient.CancelOrderByClientOrderIdAsync(cancelOrder.ClientOrderId, ct: token);
                ValidateResponse(result);
            }
            else
                throw new ArgumentException("Cancel order id is not valid");
        }

        async Task<IEnumerable<IOrder>> IPrivateConnector.GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            var openOrdersResult = await _huobiClient.GetOpenOrdersAsync(symbol: symbol, ct: token);
            ValidateResponse(openOrdersResult);
            return _converter.Convert<IEnumerable<HuobiOpenOrder>, IEnumerable<IOrder>>(openOrdersResult.Data);
        }

        async Task<IOrder> IPrivateConnector.GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(orderRequest.OrderId) && Int64.TryParse(orderRequest.OrderId, out var orderId))
            {
                var orderResult = await _huobiClient.GetOrderInfoAsync(orderId, ct: token);
                ValidateResponse(orderResult);
                return _converter.Convert<HuobiOrder, IOrder>(orderResult.Data);
            }

            throw new ArgumentException($"Order search argument is not valid for Exchange: {Exchange}");
        }

        async Task<IEnumerable<IBalance>> IPrivateConnector.GetBalanceAsync(CancellationToken token)
        {
            var balancesResult = await _huobiClient.GetBalancesAsync(_accountId, ct: token);
            ValidateResponse(balancesResult);
            return balancesResult.Data.Where(o => o.Balance != 0)
                .GroupBy(o => o.Currency).Select(o => new Balance()
                {
                    Exchange = Exchange.Huobi,
                    Currency = o.Key,
                    Available = o.FirstOrDefault(b => b.Type == HuobiBalanceType.Trade)?.Balance ?? 0,
                    Total = o.FirstOrDefault(b => b.Type == HuobiBalanceType.Trade)?.Balance ?? 0 +
                        o.FirstOrDefault(b => b.Type == HuobiBalanceType.Frozen)?.Balance ?? 0
                });
        }
    }
}