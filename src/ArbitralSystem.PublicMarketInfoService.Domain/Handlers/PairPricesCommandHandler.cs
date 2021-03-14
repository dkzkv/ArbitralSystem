using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.PublicMarketInfoService.Domain.Commands;
using ArbitralSystem.PublicMarketInfoService.Domain.Interfaces;
using ArbitralSystem.PublicMarketInfoService.Domain.Models;
using MediatR;

namespace ArbitralSystem.PublicMarketInfoService.Domain.Handlers
{
    public class PairPricesCommandHandler : IRequestHandler<SaveLastPairPricesCommand>
    {
        private readonly ILogger _logger;
        private readonly IPairPricesRepository _pairPricesRepository;
        private readonly IPublicConnectorFactory _publicConnectorFactory;

        public PairPricesCommandHandler(ILogger logger, IPairPricesRepository pairPricesRepository,
            IPublicConnectorFactory publicConnectorFactory)
        {
            Preconditions.CheckNotNull(logger, pairPricesRepository, publicConnectorFactory);

            _logger = logger;
            _pairPricesRepository = pairPricesRepository;
            _publicConnectorFactory = publicConnectorFactory;
        }

        public async Task<Unit> Handle(SaveLastPairPricesCommand request, CancellationToken token)
        {
            _logger.Information("Saving pair prices for {@exchanges} started.", request.Exchanges);
            var pricesTasks = new List<Task<IEnumerable<IPairPrice>>>();
            foreach (var exchange in request.Exchanges)
            {
                var connector = _publicConnectorFactory.GetInstance(exchange);
                pricesTasks.Add(connector.GetPairPrices(token));
            }

            var prices = (await Task.WhenAll(pricesTasks))
                .SelectMany(o => o)
                .Select(o => new PairPrice(o.ExchangePairName, o.Price, o.Exchange))
                .ToArray();
            _logger.Information($"Pair prices pulled, total: {prices.Count()}.");

            await _pairPricesRepository.BulkSave(prices, token);
            _logger.Information($"Pair prices successfully saved.");
            return Unit.Value;
        }
    }
}