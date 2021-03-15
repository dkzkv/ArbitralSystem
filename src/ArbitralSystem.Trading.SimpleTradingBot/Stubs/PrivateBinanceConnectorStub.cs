using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    internal class PrivateBinanceConnectorStub : BasePrivateConnectorStub, IPrivateConnector
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