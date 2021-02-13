using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Trading.SimpleTradingBot.Settings;
using ArbitralSystem.Trading.SimpleTradingBot.Strategies;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

namespace ArbitralSystem.Trading.SimpleTradingBot
{
    [UsedImplicitly]
    public class SimpleTradingBotService : IHostedService
    {
        private readonly IOrderBookDistributerFactory _distributerFactory;
        private readonly IPublicConnectorFactory _publicConnectorFactory;
        private readonly IPrivateConnectorFactory _privateConnectorFactory;
        private readonly ITradingStrategy _tradingStrategy;
        private readonly SimpleBotSettings _botSettings;
        private readonly ILogger _logger;

        public SimpleTradingBotService(IOrderBookDistributerFactory orderBookDistributerFactory,
            IPublicConnectorFactory publicConnectorFactory,
            IPrivateConnectorFactory privateConnectorFactory,
            ITradingStrategy tradingStrategy,
            SimpleBotSettings botSettings,
            ILogger logger)
        {
            Preconditions.CheckNotNull(orderBookDistributerFactory, publicConnectorFactory, privateConnectorFactory, botSettings, logger);
            _publicConnectorFactory = publicConnectorFactory;
            _distributerFactory = orderBookDistributerFactory;
            _privateConnectorFactory = privateConnectorFactory;
            _tradingStrategy = tradingStrategy;
            _botSettings = botSettings;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken token)
        {
            _mainCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);

            var pairInfos = await Task.WhenAll(InitializePairInfo(_botSettings.FirstExchange, _botSettings.UnificatedPairName, token),
                InitializePairInfo(_botSettings.SecondExchange, _botSettings.UnificatedPairName, token));
            _firstPair = pairInfos.First(o => o.Exchange == _botSettings.FirstExchange);
            _secondPair = pairInfos.First(o => o.Exchange == _botSettings.SecondExchange);

            await _tradingStrategy.InitializeAsync(_firstPair, _secondPair, token);

            _firstPrivateConnector = _privateConnectorFactory.GetInstance(_firstPair.Exchange);
            _secondPrivateConnector = _privateConnectorFactory.GetInstance(_secondPair.Exchange);
            
            _firstBotBalance = await GetFirstBotBalanceAsync();
            _secondBotBalance = await GetSecondBotBalanceAsync();

            _firstPrivateConnector = _privateConnectorFactory.GetInstance(_firstPair.Exchange);
            _secondPrivateConnector = _privateConnectorFactory.GetInstance(_secondPair.Exchange);

            var firstDistributor = InitializeFirstDistributor(_firstPair.Exchange);
            var firstDistributorJob = await firstDistributor.StartDistributionAsync(
                new OrderBookPairInfo(_firstPair.Exchange, _firstPair.ExchangePairName, _firstPair.UnificatedPairName), token);

            var secondDistributor = InitializeSecondDistributor(_secondPair.Exchange);
            var secondDistributorJob = await secondDistributor.StartDistributionAsync(
                new OrderBookPairInfo(_secondPair.Exchange, _secondPair.ExchangePairName, _secondPair.UnificatedPairName), token);

            await Task.WhenAll(firstDistributorJob, secondDistributorJob);
        }

        private CancellationTokenSource _mainCancellationTokenSource;

        private IOrderBook _firstOrderBook;
        private IDistributerState _firstDistributorState;

        private IOrderBook _secondOrderBook;
        private IDistributerState _secondDistributorState;

        private IPrivateConnector _firstPrivateConnector;
        private IPrivateConnector _secondPrivateConnector;

        private IPairInfo _firstPair;
        private IPairInfo _secondPair;

        private BotBalance _firstBotBalance;
        private BotBalance _secondBotBalance;

        private static object _lockObj = new object();
        private bool _isStrategyExecuting;

        private void SituationChanged()
        {
            if (IsFirstOrderBookReady() && IsSecondOrderBookReady())
            {
                if (_isStrategyExecuting == false)
                {
                    lock (_lockObj)
                    {
                        if (_isStrategyExecuting == false)
                        {
                            _isStrategyExecuting = true;
                            if (_tradingStrategy.Execute(_firstOrderBook, _secondOrderBook, out var result))
                            {
                                try
                                {
                                    var ordersResult = Task.WhenAll(_firstPrivateConnector.PlaceOrderAsync(result.FirstOrder),
                                            _secondPrivateConnector.PlaceOrderAsync(result.SecondOrder))
                                        .Result;

                                    if (!IsBalanceIncreasedAndUpdate().Result)
                                    {
                                        _logger.Warning("Price decreased!");
                                        _mainCancellationTokenSource.Cancel();
                                        return;
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.Fatal(e, "Orders not executed: {@orders}", (result.FirstOrder, result.SecondOrder));
                                    _mainCancellationTokenSource.Cancel();
                                    return;
                                }
                            }

                            _isStrategyExecuting = false;
                        }
                    }
                }
            }
        }

        private bool IsFirstOrderBookReady() => _firstOrderBook != null && _firstDistributorState.CurrentStatus == DistributerSyncStatus.Synced
                                                                        && (_firstOrderBook.BestBid.Price != 0 || _firstOrderBook.BestAsk.Price != 0);

        private bool IsSecondOrderBookReady() => _secondOrderBook != null && _secondDistributorState.CurrentStatus == DistributerSyncStatus.Synced
                                                                          && (_secondOrderBook.BestBid.Price != 0 || _secondOrderBook.BestAsk.Price != 0);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Bot stopped.");
            return Task.CompletedTask;
        }

        private async Task<bool> IsBalanceIncreasedAndUpdate()
        {
            var firstCurrentBalance = await GetFirstBotBalanceAsync();
            var secondCurrentBalance = await GetSecondBotBalanceAsync();
            var totalBase = firstCurrentBalance.BaseBalance.Total + secondCurrentBalance.BaseBalance.Total;
            var totalQuote = firstCurrentBalance.QuoteBalance.Total + secondCurrentBalance.QuoteBalance.Total;
            var prevTotalBase = _firstBotBalance.BaseBalance.Total + _secondBotBalance.BaseBalance.Total;
            var prevTotalQuote = _firstBotBalance.QuoteBalance.Total + _secondBotBalance.QuoteBalance.Total;

            _firstBotBalance = firstCurrentBalance;
            _secondBotBalance = secondCurrentBalance;

            var totalRezBase = _firstBotBalance.BaseBalance.Total + _secondBotBalance.BaseBalance.Total;
            var totalRexQuote = _firstBotBalance.QuoteBalance.Total + _secondBotBalance.QuoteBalance.Total;
            _logger.Verbose($"Balance: 1: {_firstBotBalance.BaseBalance.Total:0.####}/{_firstBotBalance.QuoteBalance.Total:0.####}, 2: {_secondBotBalance.BaseBalance.Total:0.####}/{_secondBotBalance.QuoteBalance.Total:0.####}, T: {totalRezBase:0.####}/{totalRexQuote:0.########}");
            
            return totalBase >= prevTotalBase && totalQuote >= prevTotalQuote;
        }

        private async Task<IPairInfo> InitializePairInfo(Exchange exchange, string unificatedPairName, CancellationToken token)
        {
            _logger.Information($"Pair info initialization for {exchange} started.");
            var publicConnectorFactory = _publicConnectorFactory.GetInstance(exchange);
            var pairInfo = (await publicConnectorFactory.GetPairsInfo(token))
                .FirstOrDefault(o => o.UnificatedPairName == unificatedPairName);

            if (pairInfo == null)
                throw new InvalidOperationException($"No pair: {unificatedPairName} at {exchange} exchange.");
            _logger.Information($"Pair info initialization for {exchange} finished.");
            return pairInfo;
        }

        private IOrderbookDistributor InitializeFirstDistributor(Exchange exchange)
        {
            _logger.Information($"Orderbook distributor initialization for {exchange} started.");
            var distributor = _distributerFactory.GetInstance(_botSettings.FirstExchange);
            distributor.OrderBookChanged += book =>
            {
                _firstOrderBook = book;
                SituationChanged();
            };
            distributor.DistributerStateChanged += state =>
            {
                _firstDistributorState = state;
                _logger.Debug($"State changed: {state.Exchange}, Current state: {state.CurrentStatus}");
                SituationChanged();
            };
            return distributor;
        }

        private IOrderbookDistributor InitializeSecondDistributor(Exchange exchange)
        {
            _logger.Information($"Orderbook distributor initialization for {exchange} started.");
            var distributor = _distributerFactory.GetInstance(_botSettings.FirstExchange);
            distributor.OrderBookChanged += book =>
            {
                _secondOrderBook = book;
                SituationChanged();
            };
            distributor.DistributerStateChanged += state =>
            {
                _secondDistributorState = state;
                _logger.Debug($"State changed: {state.Exchange}, Current state: {state.CurrentStatus}");
                SituationChanged();
            };
            return distributor;
        }

        private async Task<BotBalance> GetFirstBotBalanceAsync()
        {
            var balances = (await _firstPrivateConnector.GetBalanceAsync()).ToArray();
            return PrepareBotBalance(balances, _firstPair);
        }

        private async Task<BotBalance> GetSecondBotBalanceAsync()
        {
            var balances = (await _secondPrivateConnector.GetBalanceAsync()).ToArray();
            return PrepareBotBalance(balances, _secondPair);
        }

        private BotBalance PrepareBotBalance(IBalance[] balances, IPairInfo pairInfo)
        {
            var baseBalance = balances.FirstOrDefault(o => o.Currency == pairInfo.BaseCurrency);
            if (baseBalance == null)
                throw new InvalidOperationException($"No base currency balance for: {pairInfo.Exchange}");
            var quoteBalance = balances.FirstOrDefault(o => o.Currency == pairInfo.QuoteCurrency);
            if (quoteBalance == null)
                throw new InvalidOperationException($"No quote currency balance for: {pairInfo.Exchange}");
            return new BotBalance(baseBalance, quoteBalance);
        }
    }
}