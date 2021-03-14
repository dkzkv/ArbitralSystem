using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;
using ArbitralSystem.Trading.SimpleTradingBot.Stubs;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    public class PrivateConnectorFactoryStub : IPrivateConnectorFactory
    {
        private readonly IPrivateConnector _binanceStub;
        private readonly IPrivateConnector _huobiStub;

        public PrivateConnectorFactoryStub(SimpleBotSettings botSettings)
        {
            Preconditions.CheckNotNull(botSettings);
            _binanceStub = new PrivateBinanceConnectorStub(botSettings);
            _huobiStub = new PrivateHuobiConnectorStub(botSettings);
        }

        public IPrivateConnector GetInstance(Exchange exchange)
        {
            switch (exchange)
            {
                case Exchange.Binance:
                    return _binanceStub;
                case Exchange.Huobi:
                    return _huobiStub;
                default: throw new StubException("Wrong exchange");
            }
        }
    }

    public class StubMarketOrder : IPlaceOrder
    {
        public StubMarketOrder(Exchange exchange, string exchangePairName, OrderSide orderSide, decimal? price, decimal? quantity, string clientOrderId)
        {
            Exchange = exchange;
            ExchangePairName = exchangePairName;
            OrderSide = orderSide;
            OrderType = OrderType.Market;
            Price = price;
            Quantity = quantity;
            ClientOrderId = clientOrderId;
        }

        public Exchange Exchange { get; set; }
        public string ExchangePairName { get; set; }
        public OrderSide OrderSide { get; set; }
        public OrderType OrderType { get; set; }
        public decimal? Price { get; set; }
        public decimal? Quantity { get; set; }
        public string ClientOrderId { get; set; }
    }

    public class StubBalance : IBalance
    {
        public Exchange Exchange { get; set; }
        public string Currency { get; set; }
        public decimal Total { get; set; }
        public decimal Available { get; set; }
    }

    public class StubException : Exception
    {
        public StubException(string message) : base(message)
        {
        }

        public StubException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public class BasePrivateConnectorStub
    {
        public string BaseCurrency { get; }
        public string QuoteCurrency { get; }
        public BasePrivateConnectorStub(SimpleBotSettings botSettings)
        {
            var splitPair = botSettings.UnificatedPairName.Split('/');
            if(splitPair.Count() != 2)
                throw new ArgumentException($"Invalid pair format: {botSettings.UnificatedPairName} {{Base}}/{{Quote}}, for stub private connector");
            BaseCurrency = splitPair[0];
            QuoteCurrency = splitPair[1];
        }
    }
    
    public class PrivateHuobiConnectorStub : BasePrivateConnectorStub , IPrivateConnector
    {
        private StubBalance baseBalances;
        private StubBalance quoteBalances;

        public PrivateHuobiConnectorStub(SimpleBotSettings botSettings) : base(botSettings)
        {
            if( botSettings.TestBalance is null)
                 throw new ArgumentNullException("Base balance is null");
            
            baseBalances = new StubBalance()
            {
                Exchange = Exchange,
                Available = botSettings.TestBalance.Base,
                Currency = BaseCurrency.ToLower(),
                Total = botSettings.TestBalance.Base
            };
            quoteBalances = new StubBalance()
            {
                Exchange = Exchange,
                Available = botSettings.TestBalance.Quote,
                Currency = QuoteCurrency.ToLower(),
                Total = botSettings.TestBalance.Quote
            };
        }

        public Exchange Exchange => Exchange.Huobi;

        public Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token = default)
        {
            if (placeOrder.Exchange != Exchange)
                throw new StubException("Wrong exchange");

            if (placeOrder.OrderSide == OrderSide.Sell)
            {
                baseBalances.Total -= placeOrder.Quantity.Value;
                baseBalances.Available -= placeOrder.Quantity.Value;

                var profit = placeOrder.Quantity.Value * placeOrder.Price.Value;
                quoteBalances.Total += profit;
                quoteBalances.Available += profit;
            }
            else
            {
                baseBalances.Total += placeOrder.Quantity.Value;
                baseBalances.Available += placeOrder.Quantity.Value;

                var profit = placeOrder.Quantity.Value * placeOrder.Price.Value;
                quoteBalances.Total -= profit;
                quoteBalances.Available -= profit;
            }

            return Task.FromResult(string.Empty);
        }

        public Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<IBalance>> GetBalanceAsync(CancellationToken token = default)
        {
            var balances = (IEnumerable<IBalance>) new[] {baseBalances, quoteBalances};
            return Task.FromResult(balances);
        }
    }


    public class PrivateBinanceConnectorStub : BasePrivateConnectorStub, IPrivateConnector
    {
        private StubBalance baseBalances;
        private StubBalance quoteBalances;

        public PrivateBinanceConnectorStub(SimpleBotSettings botSettings) : base(botSettings)
        {
            if( botSettings.TestBalance is null)
                throw new ArgumentNullException("Base balance is null");
            baseBalances = new StubBalance()
            {
                Exchange = Exchange,
                Available = botSettings.TestBalance.Base,
                Currency = BaseCurrency.ToUpper(),
                Total = botSettings.TestBalance.Base
            };
            quoteBalances = new StubBalance()
            {
                Exchange = Exchange,
                Available = botSettings.TestBalance.Quote,
                Currency = QuoteCurrency.ToUpper(),
                Total = botSettings.TestBalance.Quote
            };
        }

        public Exchange Exchange => Exchange.Binance;

        public Task<string> PlaceOrderAsync(IPlaceOrder placeOrder, CancellationToken token = default)
        {
            if (placeOrder.Exchange != Exchange)
                throw new StubException("Wrong exchange");

            if (placeOrder.OrderSide == OrderSide.Sell)
            {
                baseBalances.Total -= placeOrder.Quantity.Value;
                baseBalances.Available -= placeOrder.Quantity.Value;

                var profit = placeOrder.Quantity.Value * placeOrder.Price.Value;
                quoteBalances.Total += profit;
                quoteBalances.Available += profit;
            }
            else
            {
                baseBalances.Total += placeOrder.Quantity.Value;
                baseBalances.Available += placeOrder.Quantity.Value;

                var profit = placeOrder.Quantity.Value * placeOrder.Price.Value;
                quoteBalances.Total -= profit;
                quoteBalances.Available -= profit;
            }

            return Task.FromResult(string.Empty);
        }

        public Task CancelOrderAsync(ICancelOrder cancelOrder, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<IOrder>> GetOpenOrdersAsync(string symbol, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IOrder> GetOrderAsync(IOrderRequest orderRequest, CancellationToken token = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<IBalance>> GetBalanceAsync(CancellationToken token = default)
        {
            var balances = (IEnumerable<IBalance>) new[] {baseBalances, quoteBalances};
            return Task.FromResult(balances);
        }
    }
}