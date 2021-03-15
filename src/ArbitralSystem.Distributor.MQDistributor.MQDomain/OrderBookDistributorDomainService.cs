using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Distributor.Core.Models;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Common;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Common.Messaging;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Interfaces;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Models;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Queries;
using ArbitralSystem.Messaging.Messages;
using AutoMapper;
using MassTransit;
using MediatR;

namespace ArbitralSystem.Distributor.MQDistributor.MQDomain
{
    public class OrderBookDistributorDomainService
    {
        private readonly IDistributorRepository _distributorRepository;
        private readonly IServerRepository _serverRepository;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly DistributorType _distributorType = DistributorType.OrderBooks;

        public OrderBookDistributorDomainService(IDistributorRepository distributorRepository,
            IServerRepository serverRepository,
            IPublishEndpoint publishEndpoint,
            IMediator mediator,
            IMapper mapper,
            ILogger logger)
        {
            _distributorRepository = distributorRepository;
            _serverRepository = serverRepository;
            _publishEndpoint = publishEndpoint;
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Guid> Create(ExchangePairInfo pairInfo, CancellationToken cancellationToken)
        {
            var existedSameDistributor =
                await _mediator.Send(new ActiveDistributorByNameQuery(pairInfo.UnificatedPairName, _distributorType), cancellationToken);
            if (existedSameDistributor != null)
                throw new InvalidOperationException($"Distributor with type: {_distributorType}, with name: {pairInfo.UnificatedPairName}, already exist.");

            var distributor =
                await _distributorRepository.CreateAsync(
                    new Models.Distributor(pairInfo.UnificatedPairName, _distributorType, pairInfo.PairInfos.Select(o => o.Exchange).ToArray()),
                    cancellationToken);

            await _publishEndpoint.Publish(
                new StartOrderBookDistribution(distributor.Id, distributor.Name, _mapper.Map<Messaging.Models.PairInfo[]>(pairInfo.PairInfos)),
                cancellationToken);

            return distributor.Id;
        }

        public async Task Update(Guid distributorId, DistributorStatus distributorStatus, Guid serverId, CancellationToken token)
        {
            var existedDistributor = await _distributorRepository.GetAsync(distributorId, token);
            if (existedDistributor is null)
            {
                string errorMessage =
                    $"Can't update distributor; type: {_distributorType}, Id: {distributorId}. Not exist.";
                _logger.Error(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (distributorStatus == DistributorStatus.Activated)
            {
                var server = await _serverRepository.GetAsync(serverId, token);

                if (existedDistributor.Server != null)
                {
                    if (existedDistributor.Server.Id != serverId)
                        _logger.Information($"Orderbook distributor changed server to: {server.Name}");
                    else
                        _logger.Warning($"Orderbook distributor changed server to the same: {server.Name}");
                }

                if (server.IsDeleted)
                    _logger.Warning("Distributor linked to disabled server!");

                existedDistributor.SetServer(server).UpdateStatus(Status.Processing);
            }
            else
            {
                existedDistributor.UpdateStatus(Status.Deleted);
            }

            await _distributorRepository.UpdateAsync(existedDistributor, token);
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var existedDistributor = await _distributorRepository.GetAsync(id, cancellationToken);
            if (existedDistributor != null && existedDistributor.Status != Status.Deleted)
            {
                await _distributorRepository.UpdateAsync(existedDistributor.UpdateStatus(Status.OnDeleting), cancellationToken);
                await _publishEndpoint.Publish(new StopOrderBookDistribution(existedDistributor.Id), cancellationToken);
            }
        }
    }
}