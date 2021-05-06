using System;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.PublicMarketInfoService.Domain.Commands;
using JetBrains.Annotations;
using MediatR;

namespace ArbitralSystem.PublicMarketInfoService.Jobs
{
    [UsedImplicitly]
    internal class PairInfoUpdaterJob
    {
        private readonly AvailableExchangesProvider _exchangesProvider;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PairInfoUpdaterJob(AvailableExchangesProvider exchangesProvider, ILogger logger, IMediator mediator)
        {
            Preconditions.CheckNotNull(exchangesProvider, mediator, logger);
            _exchangesProvider = exchangesProvider;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Execute()
        {
            _logger.Information("Pair info update job started.");
            var tokenSource = new CancellationTokenSource(new TimeSpan(0, 15, 0));
            var command = new CreateOrDelistPairsForExchangesCommand(_exchangesProvider.Get());

            await _mediator.Send(command, tokenSource.Token);
        }
    }
}