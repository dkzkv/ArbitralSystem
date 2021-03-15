using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
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
        private readonly IOrderGeneratorStrategy _orderGeneratorStrategy;
        private readonly SimpleBotSettings _botSettings;
        private readonly ILogger _logger;

        public SimpleTradingBotService(IOrderBookDistributerFactory orderBookDistributerFactory,
            IPublicConnectorFactory publicConnectorFactory,
            IPrivateConnectorFactory privateConnectorFactory,
            IOrderGeneratorStrategy orderGeneratorStrategy,
            SimpleBotSettings botSettings,
            ILogger logger)
        {
            Preconditions.CheckNotNull(orderBookDistributerFactory, publicConnectorFactory, privateConnectorFactory,orderGeneratorStrategy, botSettings, logger);
            _publicConnectorFactory = publicConnectorFactory;
            _distributerFactory = orderBookDistributerFactory;
            _privateConnectorFactory = privateConnectorFactory;
            _orderGeneratorStrategy = orderGeneratorStrategy;
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

            await _orderGeneratorStrategy.InitializeAsync(_firstPair, _secondPair, token);
            
            _firstPrivateConnector = _privateConnectorFactory.GetInstance(_firstPair.Exchange);
            _secondPrivateConnector = _privateConnectorFactory.GetInstance(_secondPair.Exchange);
            
            _firstBotBalance = await GetFirstBotBalanceAsync();
            _secondBotBalance = await GetSecondBotBalanceAsync();
            
            var firstDistributor = InitializeFirstDistributor(_firstPair.Exchange);
            var firstDistributorJob = await firstDistributor.StartDistributionAsync(
                new OrderBookPairInfo(_firstPair.Exchange, _firstPair.ExchangePairName, _firstPair.UnificatedPairName), token);

            var secondDistributor = InitializeSecondDistributor(_secondPair.Exchange);
            var secondDistributorJob = await secondDistributor.StartDistributionAsync(
                new OrderBookPairInfo(_secondPair.Exchange, _secondPair.ExchangePairName, _secondPair.UnificatedPairName), token);

            await Task.WhenAll(firstDistributorJob, secondDistributorJob);
        }

        private CancellationTokenSource _mainCancellationTokenSource;

        private IDistributorOrderBook _firstDistributorOrderBook;
        private IDistributerState _firstDistributorState;

        private IDistributorOrderBook _secondDistributorOrderBook;
        private IDistributerState _secondDistributorState;

        private IPrivateConnector _firstPrivateConnector;
        private IPrivateConnector _secondPrivateConnector;

        private IPairInfo _firstPair;
        private IPairInfo _secondPair;

        private BotBalance _firstBotBalance;
        private BotBalance _secondBotBalance;

        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _isStrategyExecuting;

        private async Task SituationChanged()
        {
            if (IsFirstOrderBookReady() && IsSecondOrderBookReady())
            {
                if (_isStrategyExecuting == false)
                {
                    await _semaphoreSlim.WaitAsync();
                    try
                    {
                        if (_isStrategyExecuting == false)
                        {
                            _isStrategyExecuting = true;
                            var orderGeneratorContext = new ExchangesContext(_firstBotBalance, _secondBotBalance,
                                _firstDistributorOrderBook, _secondDistributorOrderBook);
                            if (_orderGeneratorStrategy.Execute(orderGeneratorContext, out var generatedOrders))
                            {
                                await RunTrading(generatedOrders);
                            }

                            _isStrategyExecuting = false;
                        }
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }
            }
        }

        private async Task RunTrading((IPlaceOrder FirstOrder, IPlaceOrder SecondOrder) generatedOrders)
        {
            try
            {
                _logger.Information("Send orders: {@generatedOrders}",generatedOrders);
                await Task.WhenAll(_firstPrivateConnector.PlaceOrderAsync(generatedOrders.FirstOrder),
                    _secondPrivateConnector.PlaceOrderAsync(generatedOrders.SecondOrder));

                var balances = await Task.WhenAll(GetFirstBotBalanceAsync(), GetSecondBotBalanceAsync());
                var firstBalance = balances[0];
                var secondBalance = balances[1];

                if (!IsBalanceIncreased(firstBalance, secondBalance))
                {
                    _logger.Warning("Price decreased!");
                    _mainCancellationTokenSource.Cancel();
                    return;
                }
                UpdateBalance(firstBalance, secondBalance);
                var percent = Calculator.ComputePercent(generatedOrders.FirstOrder.Price.Value, generatedOrders.SecondOrder.Price.Value);
                _logger.Information(
                    $"Percent: {percent:0.####}, Balance: 1: {_firstBotBalance.BaseBalance.Total:0.####}/{_firstBotBalance.QuoteBalance.Total:0.####}, 2: {_secondBotBalance.BaseBalance.Total:0.####}/{_secondBotBalance.QuoteBalance.Total:0.####}, T: {(firstBalance.BaseBalance.Total + secondBalance.BaseBalance.Total):0.####}/{(firstBalance.QuoteBalance.Total + secondBalance.QuoteBalance.Total):0.########}");
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "Orders not executed: {@orders}", (generatedOrders.FirstOrder, generatedOrders.SecondOrder));
                _mainCancellationTokenSource.Cancel();
            }
        }

        private bool IsFirstOrderBookReady() => _firstDistributorOrderBook != null && _firstDistributorState.CurrentStatus == DistributerSyncStatus.Synced
                                                                        && (_firstDistributorOrderBook.BestBid.Price != 0 || _firstDistributorOrderBook.BestAsk.Price != 0);

        private bool IsSecondOrderBookReady() => _secondDistributorOrderBook != null && _secondDistributorState.CurrentStatus == DistributerSyncStatus.Synced
                                                                          && (_secondDistributorOrderBook.BestBid.Price != 0 || _secondDistributorOrderBook.BestAsk.Price != 0);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Bot stopped.");
            return Task.CompletedTask;
        }

        private void UpdateBalance(BotBalance firstCurrentBalance, BotBalance secondCurrentBalance)
        {
            _firstBotBalance = firstCurrentBalance;
            _secondBotBalance = secondCurrentBalance;
        }

        private bool IsBalanceIncreased(BotBalance firstCurrentBalance, BotBalance secondCurrentBalance)
        {
            var totalBase = firstCurrentBalance.BaseBalance.Total + secondCurrentBalance.BaseBalance.Total;
            var totalQuote = firstCurrentBalance.QuoteBalance.Total + secondCurrentBalance.QuoteBalance.Total;
            var prevTotalBase = _firstBotBalance.BaseBalance.Total + _secondBotBalance.BaseBalance.Total;
            var prevTotalQuote = _firstBotBalance.QuoteBalance.Total + _secondBotBalance.QuoteBalance.Total;

            return (totalBase >= prevTotalBase) && (totalQuote >= prevTotalQuote);
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
            distributor.OrderBookChanged += async book =>
            {
                _firstDistributorOrderBook = book;
                await SituationChanged();
            };
            distributor.DistributerStateChanged += async state =>
            {
                _firstDistributorState = state;
                _logger.Debug($"State changed: {state.Exchange}, Current state: {state.CurrentStatus}");
                await SituationChanged();
            };
            return distributor;
        }

        private IOrderbookDistributor InitializeSecondDistributor(Exchange exchange)
        {
            _logger.Information($"Orderbook distributor initialization for {exchange} started.");
            var distributor = _distributerFactory.GetInstance(_botSettings.SecondExchange);
            distributor.OrderBookChanged += async book =>
            {
                _secondDistributorOrderBook = book;
                await SituationChanged();
            };
            distributor.DistributerStateChanged += async state =>
            {
                _secondDistributorState = state;
                _logger.Debug($"State changed: {state.Exchange}, Current state: {state.CurrentStatus}");
                await SituationChanged();
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