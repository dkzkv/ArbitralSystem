using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Interfaces;
using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models;
using ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Entities;
using AutoMapper;
using EFCore.BulkExtensions;
using JetBrains.Annotations;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Repositories
{
    public class OrderBooksRepository : BaseRepository, IOrderBooksRepository
    {
        private readonly ILogger _logger;

        public OrderBooksRepository([NotNull] MarketInfoBdContext dbContext, [NotNull] IMapper mapper, [NotNull] ILogger logger)
            : base(dbContext, mapper)
        {
            Preconditions.CheckNotNull(logger);
            _logger = logger;
        }

        public async Task BulkSaveAsync(OrderBook[] orderBooks, CancellationToken cancellationToken)
        {
            var orderBookEntries = (await Prepare(orderBooks)).ToArray();
            if (orderBookEntries.Any())
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                await using (var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        await DbContext.BulkInsertAsync(orderBookEntries, cancellationToken: cancellationToken);
                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        watch.Stop();
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.Fatal(e, $"Error in Bulk insert, time for execution: {watch.ElapsedMilliseconds}");
                        throw;
                    }
                }
            }
        }

        private async Task<IEnumerable<OrderbookPriceEntry>> Prepare(OrderBook[] orderBooks)
        {
            var orderBookEntries = new ConcurrentBag<OrderbookPriceEntry>();
            foreach (var orderBook in orderBooks)
            {
                var askTask = Task.Run(() =>
                {
                    foreach (var ask in orderBook.Asks)
                        orderBookEntries.Add(FillOrderbookPriceEntry(orderBook, ask, OrderSide.Sell));
                });

                var bidTask = Task.Run(() =>
                {
                    foreach (var bid in orderBook.Bids)
                        orderBookEntries.Add(FillOrderbookPriceEntry(orderBook, bid, OrderSide.Buy));
                });
                await Task.WhenAll(askTask, bidTask);
            }

            return orderBookEntries;
        }

        private OrderbookPriceEntry FillOrderbookPriceEntry(OrderBook orderBook, OrderbookEntry entry, OrderSide orderSide)
        {
            return new OrderbookPriceEntry
            {
                UtcCatchAt = orderBook.CatchAt.UtcDateTime,
                Exchange = orderBook.Exchange,
                OrderSide = orderSide,
                Price = entry.Price,
                Quantity = entry.Quantity,
                Symbol = orderBook.Symbol
            };
        }
    }
}