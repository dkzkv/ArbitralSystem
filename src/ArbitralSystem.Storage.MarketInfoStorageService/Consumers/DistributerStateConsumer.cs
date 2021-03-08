using System;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Interfaces;
using JetBrains.Annotations;
using MassTransit;
using DistributerState = ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models.DistributerState;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Consumers
{
    [UsedImplicitly]
    internal class DistributerStateConsumer : IConsumer<IDistributerStateMessage>
    {
        private readonly IDistributerStatesRepository _distributerStatesRepository;
        private readonly ILogger _logger;
        
        private readonly TimeSpan _timeOut = TimeSpan.FromSeconds(5); 
        public DistributerStateConsumer(IDistributerStatesRepository distributerStatesRepository, ILogger logger)
        {
            Preconditions.CheckNotNull(distributerStatesRepository, logger);
            _distributerStatesRepository = distributerStatesRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IDistributerStateMessage> context)
        {
            _logger.Information("Distributor message state received: {@mess}, retry attempt: {attempt}", context.Message, context.GetRetryAttempt());

            try
            {
                var state = new DistributerState(context.Message.ClientPairId, context.Message.ChangedAt,
                    context.Message.PreviousStatus, context.Message.CurrentStatus);
                
                var result = await ArbitralStopWatch
                    .MeasureInMls(async () => await _distributerStatesRepository
                        .SaveWithNoCheckAsync(state, new CancellationTokenSource(_timeOut).Token));
                _logger.Debug($"Elapsed time for saving distributor state - {result} mls");
            }
            catch (Exception e)
            {
                _logger.Error(e,"Error while saving order book distributor state");
                throw;
            }
            
        }
    }
}