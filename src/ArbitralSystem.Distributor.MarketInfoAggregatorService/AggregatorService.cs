using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Distributor.MarketInfoAggregatorService.Models;
using ArbitralSystem.Distributor.MarketInfoAggregatorService.Services;
using ArbitralSystem.Messaging.Common;
using ArbitralSystem.Messaging.Messages;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace ArbitralSystem.Distributor.MarketInfoAggregatorService
{
    [UsedImplicitly]
    public class AggregatorService : IHostedService, IDisposable
    {
        private readonly ITimeLimitedAggregator<IOrderBookMessage> _timeLimitedAggregator;
        private readonly IBusControlFactory _busControlFactory;
        private readonly IBusControl _busControl;
        private readonly ILogger _logger;

        public AggregatorService(ITimeLimitedAggregator<IOrderBookMessage> timeLimitedAggregator,
            IBusControlFactory busControlFactory,
            IBusControl busControl,
            ILogger logger)
        {
            _timeLimitedAggregator = timeLimitedAggregator;
            _busControlFactory = busControlFactory;
            _busControl = busControl;
            _logger = logger;

            _timeLimitedAggregator.Filled += TimeLimitedAggregatorOnFilled;
        }

        private async void TimeLimitedAggregatorOnFilled(IOrderBookMessage[] objs)
        {
            try
            {
                await _busControl.Publish(new OrderBookPackageMessage(objs));
                _logger.Information($"Orderbook price entries aggregated and sent, count: {objs.Count()}");
            }
            catch (Exception e)
            {
                _logger.Error(e,"Error while sending orderbook package.");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("MarketInfo aggregation service started");
            await _busControl.StartAsync(cancellationToken);
            _timeLimitedAggregator.StartTimer();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("MarketInfo aggregation service stopped");
            await _busControl.StopAsync(cancellationToken);
            _timeLimitedAggregator.StopTimer();
            await TakeAndSendRemnant();
        }

        private async Task TakeAndSendRemnant()
        {
            var entries = _timeLimitedAggregator.Take();
            if (entries.Any())
            {
                var bus = _busControlFactory.CreateInstance();
                await bus.StartAsync();
                await bus.Publish(new OrderBookPackageMessage(entries));
                await bus.StopAsync();
            }
        }

        public void Dispose()
        {
            _timeLimitedAggregator.Filled -= TimeLimitedAggregatorOnFilled;
            _timeLimitedAggregator.Dispose();
            _logger.Information("MarketInfo aggregation service disposed");
        }
    }
}