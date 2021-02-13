using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Kucoin.Net;
using Kucoin.Net.Interfaces;
using Kucoin.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    internal class KucoinPrivateConnector : ExchangeBaseConnector , IPrivateConnector
    {
        private readonly IKucoinClient _kucoinClient;
        private readonly IConverter _converter;
        
        public KucoinPrivateConnector(ICredentials credentials, IConverter converter, IKucoinClient kucoinClient = null)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (kucoinClient == null)
                _kucoinClient = new KucoinClient(new KucoinClientOptions()
                {
                    ApiCredentials = new KucoinApiCredentials(credentials.ApiKey, credentials.SecretKey, credentials.PassPhrase)
                });
            else
                _kucoinClient = kucoinClient;

            _converter = converter;
        }

        public override Exchange Exchange => Exchange.Kucoin;
        
        async Task<string> IPrivateConnector.PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            var placedOrderResult = await _kucoinClient.PlaceOrderAsync(placeOrder.ExchangePairName,
                _converter.Convert<OrderSide, KucoinOrderSide>(placeOrder.OrderSide),
                _converter.Convert<OrderType, KucoinNewOrderType>(placeOrder.OrderType),
                quantity: placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for order should not be null, for Exchange: {Exchange}"),
                price: placeOrder.Price ?? throw new ArgumentNullException($"Price for order should not be null, for Exchange: {Exchange}"),
                clientOrderId: placeOrder.ClientOrderId,
                ct: token);
            ValidateResponse(placedOrderResult);
            return placedOrderResult.Data.OrderId;
        }

        async Task IPrivateConnector.CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token)
        {
            ValidateExchangeArgument(cancelOrder);
            if (!string.IsNullOrEmpty(cancelOrder.OrderId))
            {
                var result = await _kucoinClient.CancelOrderAsync(cancelOrder.OrderId, ct: token);
                ValidateResponse(result);
            }
            else
                throw new ArgumentException("Cancel order id is not valid");
        }

        async Task<IEnumerable<IOrder>> IPrivateConnector.GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            var totalOpenedOrders = new List<KucoinOrder>();
            var currantPage = 0;
            var pageSize = 500;
            var hasNext = false;
            do
            {
                var openedOrderResult = await _kucoinClient.GetOrdersAsync(symbol,
                    status: KucoinOrderStatus.Active,
                    currentPage: ++currantPage,
                    pageSize: pageSize,
                    ct: token);
                ValidateResponse(openedOrderResult);
                hasNext = openedOrderResult.Data.TotalItems <= pageSize * currantPage;
                totalOpenedOrders.AddRange(openedOrderResult.Data.Items);
            } while (hasNext);

            return  _converter.Convert<IEnumerable<KucoinOrder>,IEnumerable<IOrder>>(totalOpenedOrders);
        }

        async Task<IOrder> IPrivateConnector.GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(orderRequest.OrderId))
            {
                var orderResult = await _kucoinClient.GetOrderAsync(orderRequest.OrderId, token);
                ValidateResponse(orderResult);
                return _converter.Convert<KucoinOrder,IOrder>(orderResult.Data);
            }
            throw new ArgumentException($"Order search argument is not valid for Exchange: {Exchange}");
        }

        public Task<IEnumerable<IBalance>> GetBalanceAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}