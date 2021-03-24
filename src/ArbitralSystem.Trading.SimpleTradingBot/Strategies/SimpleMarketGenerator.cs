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
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models.Orders;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Trading.SimpleTradingBot.Common.Exceptions;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;
using ArbitralSystem.Trading.SimpleTradingBot.Stubs;
using JetBrains.Annotations;
using ServiceStack.Text;

namespace ArbitralSystem.Trading.SimpleTradingBot.Strategies
{
    internal class SimpleMarketGenerator : IOrderGeneratorStrategy
    {
        private readonly IAccountConnectorFactory _accountConnectorFactory;
        private readonly IPublicConnectorFactory _publicConnectorFactory;
        private readonly StrategySettings _strategySettings;
        private readonly SimpleBotSettings _botSettings;
        private readonly ILogger _logger;

        public SimpleMarketGenerator(IAccountConnectorFactory accountConnectorFactory,
            IPublicConnectorFactory publicConnectorFactory,
            SimpleBotSettings botSettings,
            StrategySettings strategySettings,
            ILogger logger)
        {
            Preconditions.CheckNotNull(accountConnectorFactory, publicConnectorFactory,botSettings, strategySettings, logger);
            _botSettings = botSettings;
            _publicConnectorFactory = publicConnectorFactory;
            _accountConnectorFactory = accountConnectorFactory;
            _strategySettings = strategySettings;
            _logger = logger;
        }
        private IPairInfo _firstPair;
        private IPairInfo _secondPair;

        private IPairCommission _firstPairCommission;
        private IPairCommission _secondPairCommission;

        private bool _isInitialized;

        private string _clientOrderIdPrefix;
        public async Task InitializeAsync(IPairInfo firstPairInfo, IPairInfo secondPairInfo, CancellationToken token)
        {
            var pairCommissions = await Task.WhenAll(InitializePairCommission(firstPairInfo, token),
                InitializePairCommission(secondPairInfo, token));

            _firstPair = firstPairInfo;
            _secondPair = secondPairInfo;
            _logger.Information("First pair: {@firstPair}, second pair: {@secondPair}", _firstPair, _secondPair);
            _firstPairCommission = pairCommissions.First(o => o.Exchange == firstPairInfo.Exchange);
            _secondPairCommission = pairCommissions.First(o => o.Exchange == secondPairInfo.Exchange);

            var firstOrderBook = await _publicConnectorFactory.GetInstance(_firstPair.Exchange).GetOrderBook(_firstPair.ExchangePairName, token);
            var secondOrderBook = await _publicConnectorFactory.GetInstance(_secondPair.Exchange).GetOrderBook(_secondPair.ExchangePairName, token);

            LogAffordableQuantitySets(_strategySettings.TradeOrderQuantity, firstPairInfo, secondPairInfo, firstOrderBook, secondOrderBook);
            _clientOrderIdPrefix = $"SPI{_firstPair.UnificatedPairName.Replace("/",String.Empty)}";
            _isInitialized = true;
        }

        public bool Execute(ExchangesContext generatorContext, out (IPlaceOrder FirstOrder, IPlaceOrder SecondOrder) result)
        {
            result = (null, null);
            ValidateOrder(generatorContext.FirstDistributorOrderBook, generatorContext.SecondDistributorOrderBook);

            var firstOrderContext = new OrderBookContext(_firstPair, generatorContext.FirstDistributorOrderBook, _firstPairCommission, generatorContext.FirstBalance);
            var secondOrderContext = new OrderBookContext(_secondPair, generatorContext.SecondDistributorOrderBook, _secondPairCommission, generatorContext.SecondBalance);

            if (firstOrderContext.DistributorOrderBook.BestBid.Price > secondOrderContext.DistributorOrderBook.BestAsk.Price &
                IsBalancesEnough(firstOrderContext.Balance, secondOrderContext.Balance, secondOrderContext.DistributorOrderBook))
            {
                if (TryGenerateOrders(firstOrderContext, secondOrderContext, out var firstOrder, out var secondOrder))
                {
                    result = (firstOrder, secondOrder);
                    return true;
                }
            }
            else if (secondOrderContext.DistributorOrderBook.BestBid.Price > firstOrderContext.DistributorOrderBook.BestAsk.Price &
                     IsBalancesEnough(secondOrderContext.Balance, firstOrderContext.Balance, firstOrderContext.DistributorOrderBook))
            {
                if (TryGenerateOrders(secondOrderContext, firstOrderContext, out var secondOrder, out var firstOrder))
                {
                    result = (firstOrder, secondOrder);
                    return true;
                }
            }

            return false;
        }

        private bool IsBalancesEnough(BotBalance firstBalance, BotBalance secondBalance, IDistributorOrderBook secondOrderbook)
        {
            var multiplier = 1.0m;
            var isEnough = firstBalance.BaseBalance.Available >= (_strategySettings.TradeOrderQuantity * multiplier)
                           && secondBalance.QuoteBalance.Available >= (_strategySettings.TradeOrderQuantity * secondOrderbook.BestAsk.Price) * multiplier;
            if(!isEnough)
              _logger.Verbose($"Balances not enough: Base-({firstBalance.BaseBalance.Available:#.000} | {_strategySettings.TradeOrderQuantity * multiplier}) Quote-({secondBalance.QuoteBalance.Available}|{(_strategySettings.TradeOrderQuantity * secondOrderbook.BestAsk.Price) * multiplier}) (second: {secondOrderbook.Exchange})");
            return isEnough;
        }

        private void ValidateOrder([NotNull] IDistributorOrderBook first, [NotNull] IDistributorOrderBook second)
        {
            if (!_isInitialized)
                throw new StrategyException("Strategy not initialized");

            if (_firstPair.Exchange != first.Exchange && _secondPair.Exchange != second.Exchange)
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

        private bool TryGenerateOrders(OrderBookContext ctx1, OrderBookContext ctx2, out IPlaceOrder firstOrder, out IPlaceOrder secondOrder)
        {
            firstOrder = null;
            secondOrder = null;
            var percent = Calculator.ComputePercent(ctx1.DistributorOrderBook.BestBid.Price, ctx2.DistributorOrderBook.BestAsk.Price);
            if (!IsOverThreshold(percent, ctx1.PairCommission, ctx2.PairCommission))
                return false;

            var quantity = GetAffordableQuantity(ctx1, ctx2);

            if (!IsValidMarketQuantity(quantity, ctx1.PairInfo, ctx2.PairInfo, ctx1.DistributorOrderBook, ctx2.DistributorOrderBook))
                return false;

            var clientOrderGuid = DateTime.Now.ToUnixTime().ToString();
            if (_botSettings.IsTestMode)
            {
                firstOrder = new StubMarketOrder(ctx1.PairInfo.Exchange, ctx1.PairInfo.ExchangePairName, OrderSide.Sell, ctx1.DistributorOrderBook.BestBid.Price, quantity,
                    GetClientId(clientOrderGuid));
                secondOrder = new StubMarketOrder(ctx2.PairInfo.Exchange, ctx2.PairInfo.ExchangePairName, OrderSide.Buy, ctx2.DistributorOrderBook.BestAsk.Price, quantity,
                    GetClientId(clientOrderGuid));
            }
            else
            {
                firstOrder = new MarketOrder(ctx1.PairInfo.ExchangePairName, OrderSide.Sell, quantity, GetClientId(clientOrderGuid), ctx1.PairInfo.Exchange);
                secondOrder = new MarketOrder(ctx2.PairInfo.ExchangePairName, OrderSide.Buy, quantity, GetClientId(clientOrderGuid), ctx2.PairInfo.Exchange);
            }
            return true;
        }

        private decimal GetAffordableQuantity(OrderBookContext ctx1, OrderBookContext ctx2)
        {
            var minExchangeQuantity = ctx1.DistributorOrderBook.BestBid.Quantity < ctx2.DistributorOrderBook.BestBid.Quantity
                ? ctx1.DistributorOrderBook.BestBid.Quantity
                : ctx2.DistributorOrderBook.BestBid.Quantity;
            return minExchangeQuantity > _strategySettings.TradeOrderQuantity ? _strategySettings.TradeOrderQuantity : minExchangeQuantity;
        }

        private string GetClientId(string clientOrderId) => _clientOrderIdPrefix + clientOrderId;

        private bool IsOverThreshold(decimal percent, IPairCommission first, IPairCommission second)
        {
            var commission = (first.MakerPercent + first.TakerPercent + second.MakerPercent + second.TakerPercent) +
                             _strategySettings.PercentThresholdExtension;

            if (percent > _maxPercent )
                _maxPercent = percent;
            _logger.Verbose($"Percent: {Math.Abs(percent):0.###}, Threshold: {commission:0.###}, Max: {_maxPercent:0.###}");
            return percent > commission;
        }

        private static decimal _maxPercent = 0m;
        
        private bool IsValidMarketQuantity(decimal quantity, IPairInfo firstPairInfo, IPairInfo secondPairInfo,
            IOrderBook firstDistributorOrderBook, IOrderBook secondDistributorOrderBook)
        {
            var firstMinLimitOrderValue = quantity * firstDistributorOrderBook.BestBid.Price;
            var secondMinLimitOrderValue = quantity * secondDistributorOrderBook.BestAsk.Price;
            bool isValid = quantity > firstPairInfo.MinMarketOrderAmount &&
                           quantity > secondPairInfo.MinMarketOrderAmount &&
                           firstMinLimitOrderValue > firstPairInfo.MinLimitOrderValue &&
                           secondMinLimitOrderValue > secondPairInfo.MinLimitOrderValue;

            if (!isValid)
                _logger.Warning($"Quantity: {quantity} not valid.");

            return isValid;
        }

        private void LogAffordableQuantitySets(decimal quantity, IPairInfo firstPairInfo, IPairInfo secondPairInfo,
            IOrderBook firstDistributorOrderBook, IOrderBook secondDistributorOrderBook)
        {
            var firstMinLimitOrderValue = quantity * firstDistributorOrderBook.BestBid.Price;
            var secondMinLimitOrderValue = quantity * secondDistributorOrderBook.BestAsk.Price;
            var message =
                $"Quantity: {quantity}, MinMarketOrder F: {firstPairInfo.MinMarketOrderAmount}, S: {secondPairInfo.MinMarketOrderAmount}; MinLimitOrderValue: F: {firstMinLimitOrderValue} / {firstPairInfo.MinLimitOrderValue}, S: {secondMinLimitOrderValue} / {secondPairInfo.MinLimitOrderValue}";
            
            _logger.Information(message);
        }
    }
}