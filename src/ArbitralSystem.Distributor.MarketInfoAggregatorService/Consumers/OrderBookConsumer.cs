using System.Linq;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Distributor.MarketInfoAggregatorService.Services;
using ArbitralSystem.Messaging.Messages;
using JetBrains.Annotations;
using MassTransit;

namespace ArbitralSystem.Distributor.MarketInfoAggregatorService.Consumers
{
    [UsedImplicitly]
    internal class OrderBookConsumer : IConsumer<IOrderBookMessage>
    {
        private readonly ITimeLimitedAggregator<IOrderBookMessage> _timeLimitedAggregator;
        private readonly ILogger _logger;

        public OrderBookConsumer(ITimeLimitedAggregator<IOrderBookMessage> timeLimitedAggregator,
                                 ILogger logger)
        {
            _timeLimitedAggregator = timeLimitedAggregator;
            _logger = logger;
        }
        
        public Task Consume(ConsumeContext<IOrderBookMessage> context)
        {
            var orderBook = context.Message;
            _logger.Debug($"Orderbook: {orderBook.ClientPairId} [{orderBook.Bids.Count()}/{orderBook.Asks.Count()}]");
            _timeLimitedAggregator.Add(context.Message);
            return Task.CompletedTask;
        }
    }
}