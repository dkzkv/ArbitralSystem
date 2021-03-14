using System;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Distributor.Core.Interfaces;
using ArbitralSystem.Messaging.Messages;
using AutoMapper;
using MassTransit;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging
{
    internal class OrderBookPublisher : IOrderBookPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        
        public OrderBookPublisher(IPublishEndpoint publishEndpoint,ILogger logger, IMapper mapper)
        {
            Preconditions.CheckNotNull(publishEndpoint, mapper,logger);
            _mapper = mapper;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }
        
        public async Task Publish(IDistributorOrderBook orderbook)
        {
            try
            {
                await _publishEndpoint.Publish(_mapper.Map<IOrderBookMessage>(orderbook));
            }
            catch (Exception e)
            {
                _logger.Error(e,"Error while publishing order book.");
            }
        }

        public async Task Publish(IDistributerState orderBookState)
        {
            await _publishEndpoint.Publish(_mapper.Map<IDistributerStateMessage>(orderBookState));
        }
    }
}