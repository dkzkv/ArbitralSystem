using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Distributor.Core.Common;
using ArbitralSystem.Distributor.Core.Interfaces;
using ArbitralSystem.Distributor.Core.Models;
using AutoMapper;
using Polly;
using Polly.Retry;

namespace ArbitralSystem.Distributor.Core.Jobs
{
    public class OrderBookDistributorJob
    {
        private readonly IOrderBookDistributerFactory _orderBookDistributer;
        private readonly DistributionOptions _distributionOptions;
        private readonly IOrderBookPublisher _publisher;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private Func<IDistributorOrderBook, Task> _heartBeat;

        public OrderBookDistributorJob(IOrderBookDistributerFactory orderBookDistributer,
            DistributionOptions distributionOptions,
            IOrderBookPublisher publisher,
            ILogger logger,
            IMapper mapper)
        {
            Preconditions.CheckNotNull(orderBookDistributer, distributionOptions, publisher, logger, mapper);
            _orderBookDistributer = orderBookDistributer;
            _distributionOptions = distributionOptions;
            _publisher = publisher;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task Distribute(ExchangePairInfo exchangePairsInfo, Func<IDistributorOrderBook, Task> heartBeat, CancellationToken token)
        {
            _heartBeat = heartBeat;
            await Distribute(exchangePairsInfo, token);
        }

        public async Task Distribute(ExchangePairInfo exchangePairsInfo, CancellationToken token)
        {
            var cancellationDistributionTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var availableDistributers = new List<IOrderbookDistributor>();
            try
            {
                var availablePairs = exchangePairsInfo.PairInfos.ToArray();
                _logger.Debug("Total available pairs: {count}, Extended: {exchanges}"
                    , availablePairs.Count(), String.Join(" , ", availablePairs.Select(o => o.Exchange).ToArray()));

                availableDistributers = GetAvailableDistributers(availablePairs);

                _logger.Debug("Start distribution");
                await StartDistribution(availableDistributers, availablePairs, cancellationDistributionTokenSource.Token);
            }
            catch (TaskCanceledException tEx)
            {
                cancellationDistributionTokenSource.Cancel();
                _logger.Warning(tEx, "Task canceled while distribution in core");
            }
            catch (Exception ex)
            {
                cancellationDistributionTokenSource.Cancel();
                _logger.Fatal(ex, "Fatal error while distribution in core");
                throw;
            }
            finally
            {
                _logger.Information("Unsubscription started.");
                if (availableDistributers.Any())
                    UnSubscribeFromDistributers(availableDistributers);
                _logger.Information("Stopped distribution");
            }
        }

        private async void Instance_OrderBookChanged(IDistributorOrderBook distributorOrderBook)
        {
            var currentOrderBook = _distributionOptions.TrimOrderBookDepth.HasValue
                ? OrderbookTrimmer.Trim(distributorOrderBook, _distributionOptions.TrimOrderBookDepth.Value)
                : distributorOrderBook;
            
            await _publisher.Publish(currentOrderBook);
            if (_heartBeat != null)
                await _heartBeat.Invoke(currentOrderBook);
            _logger.Debug(
                $"Orderbook updated for pair {currentOrderBook.Symbol}, asks:{currentOrderBook.Asks.Count()}, bids:{currentOrderBook.Bids.Count()}, from {currentOrderBook.Exchange}");
        }

        private async void Instance_DistributerStateChanged(IDistributerState state)
        {
            await _publisher.Publish(state);
            _logger.Information("distributer state updated: {@state}", state);
        }

        private List<IOrderbookDistributor> GetAvailableDistributers(IEnumerable<PairInfo> availablePairs)
        {
            var availableDistributers = new List<IOrderbookDistributor>();
            foreach (var pair in availablePairs)
            {
                var instance = _orderBookDistributer.GetInstance(pair.Exchange);
                instance.OrderBookChanged += Instance_OrderBookChanged;
                instance.DistributerStateChanged += Instance_DistributerStateChanged;

                _logger.Debug("Preparing distribution for Exchange: {@exchanges}", pair.Exchange);

                availableDistributers.Add(instance);
            }

            return availableDistributers;
        }

        private async Task StartDistribution(IEnumerable<IOrderbookDistributor> distributers, PairInfo[] pairs, CancellationToken token)
        {
            var activeDistributers = new List<Task>();
            foreach (var distributor in distributers)
            {
                PairInfo pair = pairs.SingleOrDefault(o => o.Exchange == distributor.Exchange);
                var distributerPair = _mapper.Map<OrderBookPairInfo>(pair);

                var distributerTask = await Policy.Handle<WebsocketException>()
                    .WaitAndRetryAsync(10, count => TimeSpan.FromSeconds(5),
                        (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.Warning(exception, $"Error while starting distribution: {retryCount} - retry count");
                        })
                    .ExecuteAsync(async () => await distributor.StartDistributionAsync(distributerPair, token));

                activeDistributers.Add(distributerTask);
            }

            await Task.WhenAll(activeDistributers);
        }

        private void UnSubscribeFromDistributers(List<IOrderbookDistributor> distributers)
        {
            foreach (var distributer in distributers)
            {
                distributer.OrderBookChanged -= Instance_OrderBookChanged;
                distributer.DistributerStateChanged -= Instance_DistributerStateChanged;
            }
        }
    }
}