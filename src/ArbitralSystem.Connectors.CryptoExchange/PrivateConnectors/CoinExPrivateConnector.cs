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
using CoinEx.Net;
using CoinEx.Net.Interfaces;
using CoinEx.Net.Objects;
using CryptoExchange.Net.Objects;
using OrderType = ArbitralSystem.Domain.MarketInfo.OrderType;
namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    internal class CoinExPrivateConnector : ExchangeBaseConnector , IPrivateConnector
    {
        private readonly ICoinExClient _coinExClient;
        private readonly IConverter _converter;

        public CoinExPrivateConnector(ICredentials credentials, IConverter converter, ICoinExClient coinExClient = null)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (coinExClient == null)
                _coinExClient = new CoinExClient();
            else
                _coinExClient = coinExClient;

            _converter = converter;
            _coinExClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }
        
        public override Exchange Exchange => Exchange.CoinEx;
        public async Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            WebCallResult<CoinExOrder> orderResult = null;
            if (placeOrder.OrderType == OrderType.Market)
            {
                orderResult = await _coinExClient.PlaceMarketOrderAsync(placeOrder.ExchangePairName,
                    _converter.Convert<OrderType ,TransactionType>(placeOrder.OrderType),
                    placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for market order should not be null, for Exchange: {Exchange}"),
                    sourceId: placeOrder.ClientOrderId,
                    ct: token);
            }
            else if(placeOrder.OrderType == OrderType.Limit)
            {
                orderResult = await _coinExClient.PlaceLimitOrderAsync(placeOrder.ExchangePairName,
                    _converter.Convert<OrderType ,TransactionType>(placeOrder.OrderType),
                    placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for limit order should not be null, for Exchange: {Exchange}"),
                    price: placeOrder.Price ?? throw new ArgumentNullException($"Price for limit order should not be null, for Exchange: {Exchange}"),
                    sourceId: placeOrder.ClientOrderId,
                    ct: token);
            }
            else
            {
                throw new NotSupportedException($"Not supported order type: {placeOrder.OrderType} for Exchange: {Exchange}");
            }
            ValidateResponse(orderResult);
            return orderResult.Data.Id.ToString();
        }

        public async Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token)
        {
            ValidateExchangeArgument(cancelOrder);
            if (!string.IsNullOrEmpty(cancelOrder.OrderId) && Int64.TryParse(cancelOrder.OrderId, out var orderId))
            {
                var result = await _coinExClient.CancelOrderAsync(cancelOrder.Symbol, orderId, ct: token);
                ValidateResponse(result);
            }
            else
                throw new ArgumentException("Cancel order id is not valid");
        }

        public async Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            var coinExTotalOpenOrders = new List<CoinExOrder>();
            var counter = 1;
            var hasNext = false;
            do
            {
                var openOrdersResult = await _coinExClient.GetOpenOrdersAsync(symbol, counter++, 100, ct: token);
                ValidateResponse(openOrdersResult);
                hasNext = openOrdersResult.Data.HasNext;
                coinExTotalOpenOrders.AddRange(openOrdersResult.Data.Data);
            } while (hasNext);
            
            return _converter.Convert<IEnumerable<CoinExOrder>,IEnumerable<IOrder>>(coinExTotalOpenOrders);
        }

        public async Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            if(!string.IsNullOrEmpty(orderRequest.OrderId) && Int64.TryParse(orderRequest.OrderId, out var orderId))
            {
                var orderResult = await _coinExClient.GetOrderStatusAsync(orderId, orderRequest.Symbol, token);
                ValidateResponse(orderResult);
                return _converter.Convert<CoinExOrder,IOrder>(orderResult.Data);
            }
            throw new ArgumentException($"Order search argument is not valid for Exchange: {Exchange}");
        }

        public Task<IEnumerable<IBalance>> GetBalanceAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}