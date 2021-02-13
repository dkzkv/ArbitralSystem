using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Kraken.Net;
using Kraken.Net.Interfaces;
using Kraken.Net.Objects;
using Arbitral = ArbitralSystem.Domain.MarketInfo;
using OrderSide = Kraken.Net.Objects.OrderSide;
using OrderType = Kraken.Net.Objects.OrderType;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    internal class KrakenPrivateConnector : ExchangeBaseConnector , IPrivateConnector
    {
        private readonly IKrakenClient _krakenClient;
        private readonly IConverter _converter;
        
        public KrakenPrivateConnector(ICredentials credentials, IConverter converter, IKrakenClient krakenClient = null)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (krakenClient == null)
                _krakenClient = new KrakenClient();
            else
                _krakenClient = krakenClient;

            _converter = converter;
            _krakenClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        public override Exchange Exchange => Exchange.Kraken;
        
        async Task<string> IPrivateConnector.PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            uint? krakenClientOrderId = null;
            if (!string.IsNullOrEmpty(placeOrder.ClientOrderId))
            {
                if (Int64.TryParse(placeOrder.ClientOrderId, out var orderId))
                    krakenClientOrderId = (uint?) orderId;
                else
                    throw new ArgumentException($"Can't parse client order id for: {Exchange}, id should be 'long'");
            }
            
            var placedResult = await _krakenClient.PlaceOrderAsync(placeOrder.ExchangePairName,
                _converter.Convert<Arbitral.OrderSide ,OrderSide>(placeOrder.OrderSide),
                _converter.Convert<Arbitral.OrderType ,OrderType>(placeOrder.OrderType),
                quantity: placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for order should not be null, for Exchange: {Exchange}"),
                price: placeOrder.Price ?? throw new ArgumentNullException($"Price order should not be null, for Exchange: {Exchange}"),
                clientOrderId: krakenClientOrderId,
                ct: token);
            ValidateResponse(placedResult);
            if(placedResult.Data.OrderIds.Count() != 1)
                throw new RestClientException("Not expected placed order response. Not one id.");
            return placedResult.Data.OrderIds.First();
        }
        
        async Task IPrivateConnector.CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token)
        {
            ValidateExchangeArgument(cancelOrder);
            if (!string.IsNullOrEmpty(cancelOrder.OrderId))
            {
                var result = await _krakenClient.CancelOrderAsync(cancelOrder.OrderId, ct: token);
                ValidateResponse(result);
            }
            else
                throw new ArgumentException("Cancel order id is not valid");
        }

        async Task<IEnumerable<IOrder>> IPrivateConnector.GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            var openedOrderResult = await _krakenClient.GetOpenOrdersAsync(ct: token);
            ValidateResponse(openedOrderResult);
            var rawOrders = openedOrderResult.Data.Open.Where(o => o.Value.OrderDetails.Symbol == symbol).Select(o=>o.Value);
            return _converter.Convert<IEnumerable<KrakenOrder>,IEnumerable<IOrder>>(rawOrders);
        }

        async Task<IOrder> IPrivateConnector.GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(orderRequest.ClientOrderId))
            {
                var openedOrders = await _krakenClient.GetOpenOrdersAsync(orderRequest.ClientOrderId, ct: token);
                ValidateResponse(openedOrders);
                if (openedOrders.Data.Open.Count == 1)
                    return _converter.Convert<KrakenOrder,IOrder>(openedOrders.Data.Open.Select(o=>o.Value).First());
                else if(openedOrders.Data.Open.Count > 1)
                    throw new RestClientException($"Unexpected amount of orders by client order Id {orderRequest.ClientOrderId}");
                
                var closedOrders = await _krakenClient.GetClosedOrdersAsync(orderRequest.ClientOrderId, ct: token);
                ValidateResponse(closedOrders);
                if (openedOrders.Data.Open.Count == 1)
                    return _converter.Convert<KrakenOrder,IOrder>(openedOrders.Data.Open.Select(o=>o.Value).First());
                
                throw new RestClientException($"Unexpected amount of orders by client order Id {orderRequest.ClientOrderId}");
            }
            throw new ArgumentException($"Order search argument is not valid for Exchange: {Exchange}");
        }

        public Task<IEnumerable<IBalance>> GetBalanceAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}