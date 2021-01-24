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
    public class CoinExTradingConnector : ExchangeBaseConnector , ITradingConnector
    {
        private readonly ICoinExClient _coinExClient;
        private readonly IConverter _converter;

        public CoinExTradingConnector(ICredentials credentials, IConverter converter, ICoinExClient coinExClient = null)
        {
            Preconditions.CheckNotNull(credentials, converter);
            if (coinExClient == null)
                _coinExClient = new CoinExClient();
            else
                _coinExClient = coinExClient;

            _converter = converter;
            _coinExClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }
        
        public async Task PlaceOrderAsync(IPlaceOrder placeOrder)
        {


        }

        public async Task CancelOrderAsync(string symbol, string orderId)
        {
            var canceledOrder = await _coinExClient.CancelOrderAsync(symbol, Int64.Parse(orderId));
        }

        public async Task GetOpenOrderAsync(string symbol)
        {
            
        }

        public override Exchange Exchange => Exchange.CoinEx;
        public async Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token)
        {
            ValidateExchangeArgument(placeOrder);
            WebCallResult<CoinExOrder> orderResult = null;
            if (placeOrder.OrderType == OrderType.Market)
            {
                orderResult = await _coinExClient.PlaceMarketOrderAsync(placeOrder.Symbol,
                    _converter.Convert<OrderType ,TransactionType>(placeOrder.OrderType),
                    placeOrder.Quantity ?? throw new ArgumentNullException($"Quantity for marker order should not be null, for Exchange: {Exchange}"),
                    sourceId: placeOrder.ClientOrderId,
                    ct: token);
            }
            else if(placeOrder.OrderType == OrderType.Limit)
            {
                orderResult = await _coinExClient.PlaceLimitOrderAsync(placeOrder.Symbol,
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
            if (!string.IsNullOrEmpty(cancelOrder.Id) && Int64.TryParse(cancelOrder.Id, out var orderId))
            {
                var result = await _coinExClient.CancelOrderAsync(cancelOrder.Symbol, orderId, ct: token);
                ValidateResponse(result);
            }
        }

        public async Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token)
        {
            var openOrders = await _coinExClient.GetOpenOrdersAsync(symbol, 0, 100, ct: token);
            ValidateResponse(openOrders);
            return _converter.Convert<IEnumerable<CoinExOrder>,IEnumerable<IOrder>>(openOrders.Data.);
        }

        public Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token)
        {
            throw new NotImplementedException();
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