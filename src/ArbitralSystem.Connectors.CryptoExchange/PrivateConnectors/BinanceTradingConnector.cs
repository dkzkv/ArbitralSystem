using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class BinanceTradingConnector : BaseConnector , ITradingConnector
    {
        private readonly IBinanceClient _binanceClient;

        public BinanceTradingConnector(ICredentials credentials, IBinanceClient binanceClient = null)
        {
            if (binanceClient == null)
                _binanceClient = new BinanceClient();
            else
                _binanceClient = binanceClient;

            _binanceClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        public Exchange Exchange => Exchange.Binance;

        public async Task<string> PlaceOrderAsync(IOrder order, CancellationToken token)
        {
            var placedOrderResult = await _binanceClient.Spot.Order.PlaceTestOrderAsync(symbol, OrderSide.Buy, OrderType.Market, size , ct: token);
            ValidateResponse(placedOrderResult);
            return placedOrderResult.Data.OrderId.ToString();
        }
        
        public async Task CancelOrderAsync(string symbol,string orderId, CancellationToken token)
        {
            WebCallResult<BinanceCanceledOrder> result = await _binanceClient.Spot.Order.CancelOrderAsync(symbol, Int64.Parse(orderId));
        }
        
        public async Task GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
           WebCallResult<IEnumerable<BinanceOrder>> result = await _binanceClient.Spot.Order.GetOpenOrdersAsync(symbol);
        }
        
        public async Task GetOpenOrderAsync(string symbol, CancellationToken token)
        {
           WebCallResult<BinanceOrder> result = await _binanceClient.Spot.Order.GetOrderAsync(symbol);
        }

        
    }
}