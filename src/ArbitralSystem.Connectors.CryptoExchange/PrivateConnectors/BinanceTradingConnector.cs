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
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;
using OrderSide = Binance.Net.Enums.OrderSide;
using OrderType = Binance.Net.Enums.OrderType;
using Arbitral = ArbitralSystem.Domain.MarketInfo;
namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class BinanceTradingConnector : ExchangeBaseConnector , ITradingConnector
    {
        private readonly IBinanceClient _binanceClient;
        private readonly IConverter _converter;

        public BinanceTradingConnector(ICredentials credentials, IConverter converter, IBinanceClient binanceClient = null)
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
        
        public async Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            var placedOrderResult = await _binanceClient.Spot.Order.PlaceTestOrderAsync(placeOrder.Symbol,
                _converter.Convert<Arbitral.OrderSide, OrderSide>(placeOrder.OrderSide),
                _converter.Convert<Arbitral.OrderType, OrderType>(placeOrder.OrderType),
                placeOrder.Quantity,
                placeOrder.Price,
                newClientOrderId: placeOrder.ClientOrderId,
                ct: token);
            
            ValidateResponse(placedOrderResult);
            return placedOrderResult.Data.OrderId.ToString();
        }

        public async Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token)
        {
            ValidateExchangeArgument(cancelOrder);
            if (!string.IsNullOrEmpty(cancelOrder.Id) && Int64.TryParse(cancelOrder.Id, out var orderId))
            {
                var result = await _binanceClient.Spot.Order.CancelOrderAsync(cancelOrder.Symbol, orderId, ct: token);
                ValidateResponse(result);
            }
            throw new ArgumentException("Cancel order id is not valid");
        }
        
        public async Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
          var openOrdersAsync = await _binanceClient.Spot.Order.GetOpenOrdersAsync(symbol, ct: token);
          ValidateResponse(openOrdersAsync);
          return _converter.Convert<IEnumerable<BinanceOrder>,IEnumerable<IOrder>>(openOrdersAsync.Data);
        }
        
        public async Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            WebCallResult<BinanceOrder> orderResult = null;
            if (!string.IsNullOrEmpty(orderRequest.OrderId) && Int64.TryParse(orderRequest.OrderId, out var orderId))
            {
                orderResult = await _binanceClient.Spot.Order.GetOrderAsync(orderRequest.Symbol, orderId, ct:token);
            }
            else if (!string.IsNullOrEmpty(orderRequest.ClientOrderId))
            {
                orderResult = await _binanceClient.Spot.Order.GetOrderAsync(orderRequest.Symbol, origClientOrderId: orderRequest.ClientOrderId, ct:token);
            }
            else
            {
                throw new ArgumentException($"Order search argument is not valid for Exchange: {Exchange}");
            }
            ValidateResponse(orderResult);
            return _converter.Convert<BinanceOrder,IOrder>(orderResult.Data);
        }
    }
}