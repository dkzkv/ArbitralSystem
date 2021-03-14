using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Interfaces;
using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models;
using AutoMapper;
using JetBrains.Annotations;
using MassTransit;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Consumers
{
    [UsedImplicitly]
    internal class OrderBookConsumer : IConsumer<IOrderBookPackageMessage>
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IOrderBooksRepository _orderBooksRepository;
        private readonly TimeSpan _timeOut = TimeSpan.FromSeconds(5*60); 
        
        public OrderBookConsumer(IOrderBooksRepository orderBooksRepository,IMapper mapper, ILogger logger)
        {
            Preconditions.CheckNotNull(orderBooksRepository, logger, mapper);
            _orderBooksRepository = orderBooksRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task Consume(ConsumeContext<IOrderBookPackageMessage> context)
        {
            _logger.Information("Order book package message received: {id}, retry attempt: {attempt}", context.CorrelationId, context.GetRetryAttempt());
            
            try
            {
                var orderBooks = _mapper.Map<IEnumerable<OrderBook>>(context.Message.OrderBooks).ToArray();
                if (orderBooks.Any())
                {
                    var result = await ArbitralStopWatch
                        .MeasureInMls(async () => await _orderBooksRepository.BulkSaveAsync(orderBooks,
                            new CancellationTokenSource(_timeOut).Token));
                    _logger.Debug($"Elapsed time for saving orderbooks {result} mls");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e,"Error while saving distributor order books");
                throw;
            }

        }
    }
}