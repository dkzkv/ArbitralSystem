using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Account;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models.Orders;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Trading.SimpleTradingBot.Common.Exceptions;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;
using ArbitralSystem.Trading.SimpleTradingBot.Stubs;
using JetBrains.Annotations;

namespace ArbitralSystem.Trading.SimpleTradingBot.Strategies
{
    internal class SimpleMarket : ITradingStrategy
    {
        private readonly IAccountConnectorFactory _accountConnectorFactory;
        private readonly StrategySettings _strategySettings;
        private readonly ILogger _logger;
        
        public SimpleMarket(IAccountConnectorFactory accountConnectorFactory,StrategySettings strategySettings, ILogger logger)
        {
            Preconditions.CheckNotNull(accountConnectorFactory,strategySettings, logger);
            _accountConnectorFactory = accountConnectorFactory;
            _strategySettings = strategySettings;
            _logger = logger;
        }

        private int _clientOrderIdCounter;
        
        private IPairInfo _firstPair;
        private IPairInfo _secondPair;
        
        private IPairCommission _firstPairCommission;
        private IPairCommission _secondPairCommission;

        private bool _isInitialized;
        
        public async Task InitializeAsync(IPairInfo firstPairInfo, IPairInfo secondPairInfo, CancellationToken token)
        {
            var pairCommissions = await Task.WhenAll(InitializePairCommission(firstPairInfo, token),
                InitializePairCommission(secondPairInfo, token));

            _firstPair = firstPairInfo;
            _secondPair = secondPairInfo;
            _firstPairCommission = pairCommissions.First(o => o.Exchange == firstPairInfo.Exchange);
            _secondPairCommission = pairCommissions.First(o => o.Exchange == secondPairInfo.Exchange);
            _isInitialized = true;
        }

        public bool Execute(IOrderBook first, IOrderBook second, out (IPlaceOrder FirstOrder, IPlaceOrder SecondOrder) result)
        {
            result = (null, null);
            ValidateOrder(first, second);
            var firstOrderContext = new OrderBookContext(_firstPair, first, _firstPairCommission);
            var secondOrderContext = new OrderBookContext(_secondPair, second, _secondPairCommission);
            if (first.BestBid.Price > second.BestAsk.Price)
            {
                if (TryGenerateOrders(firstOrderContext, secondOrderContext, out var firstOrder, out var secondOrder))
                {
                    result = (firstOrder, secondOrder);
                    return true;
                }
            }
            else if (second.BestBid.Price > first.BestAsk.Price)
            {
                if (TryGenerateOrders(secondOrderContext, firstOrderContext, out var secondOrder, out var firstOrder))
                {
                    result = (firstOrder,secondOrder );
                    return true;
                }
            }
            return false;
        }

        private void ValidateOrder([NotNull]IOrderBook first,[NotNull] IOrderBook second)
        {
            if (!_isInitialized)
                throw new StrategyException("Strategy not initialized");
            
            if(_firstPair.Exchange != first.Exchange && _secondPair.Exchange != second.Exchange)
                throw new StrategyException("Wrong order of arguments");
        }
        
        private async Task<IPairCommission> InitializePairCommission(IPairInfo pairInfo, CancellationToken token)
        {
            _logger.Information($"Pair commission initialization for {pairInfo.Exchange} started.");
            var firstAccountFactory = _accountConnectorFactory.GetInstance(pairInfo.Exchange);
            var commission = await firstAccountFactory.GetFeeRateAsync(pairInfo.ExchangePairName, token);

            _logger.Information($"Pair commission initialization for {pairInfo.Exchange} finished.");
            return commission;
        }
        
        private bool TryGenerateOrders(OrderBookContext ctx1 , OrderBookContext ctx2, out IPlaceOrder firstOrder, out IPlaceOrder secondOrder)
        {
            firstOrder = null;
            secondOrder = null;
            var percent = Calculator.ComputePercent(ctx1.OrderBook.BestBid.Price, ctx2.OrderBook.BestAsk.Price);
            if (!IsOverThreshold(percent, ctx1.PairCommission, ctx2.PairCommission))
                return false;
                
            var quantity = ctx1.OrderBook.BestBid.Quantity > _strategySettings.TradeOrderQuantity 
                ? _strategySettings.TradeOrderQuantity : ctx1.OrderBook.BestBid.Quantity;
            
            if (!IsValidMarketQuantity(quantity, ctx1, ctx2))
                return false;
            
            firstOrder = new StubMarketOrder(ctx1.PairInfo.Exchange, ctx1.PairInfo.ExchangePairName, OrderSide.Sell, ctx1.OrderBook.BestBid.Price ,quantity, GetClientId(_clientOrderIdCounter));
            secondOrder = new StubMarketOrder( ctx2.PairInfo.Exchange ,ctx2.PairInfo.ExchangePairName, OrderSide.Buy,ctx2.OrderBook.BestAsk.Price ,quantity, GetClientId(_clientOrderIdCounter));
            //firstOrder = new MarketOrder(ctx1.PairInfo.ExchangePairName, OrderSide.Sell, quantity, GetClientId(_clientOrderIdCounter), ctx1.PairInfo.Exchange);
            //secondOrder = new MarketOrder(ctx2.PairInfo.ExchangePairName, OrderSide.Buy, quantity, GetClientId(_clientOrderIdCounter), ctx2.PairInfo.Exchange);
            _clientOrderIdCounter++;
            return true;
        }

        private string GetClientId(int clientOrderCounter) => $"simple_trading_bot_{clientOrderCounter}";
        
        private bool IsOverThreshold(decimal percent, IPairCommission first, IPairCommission second)
        {
            //var commission = (first.MakerPercent + first.TakerPercent + second.MakerPercent + second.TakerPercent);
            var commission = 0.01m;
            _logger.Verbose($"Percent: {Math.Abs(percent):0.###}, Threshold: {commission:0.###}");
            return percent > commission;
        }

        private bool IsValidMarketQuantity(decimal quantity , OrderBookContext first, OrderBookContext second)
        {
            bool isValid = quantity > first.PairInfo.MinMarketOrderAmount &&
                   quantity > second.PairInfo.MinMarketOrderAmount &&
                   first.OrderBook.BestBid.Quantity * first.OrderBook.BestBid.Price > first.PairInfo.MinLimitOrderValue &&
                   second.OrderBook.BestAsk.Quantity * second.OrderBook.BestAsk.Price > second.PairInfo.MinLimitOrderValue;
            
            if(!isValid)
                _logger.Verbose($"Quantity: {quantity} not valid.");

            return isValid;
        }
    }
}