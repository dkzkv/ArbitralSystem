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
                if (!orderBook.Asks.Any() && !orderBook.Bids.Any())
                {
                    orderBookEntries.Add(FillEmptyBuyOrderbookPriceEntry(orderBook));
                    orderBookEntries.Add(FillEmptySellOrderbookPriceEntry(orderBook));
                    continue;
                }

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
                ClientPairId = orderBook.ClientPairId,
                UtcCatchAt = orderBook.CatchAt.UtcDateTime,
                OrderSide = orderSide,
                Price = entry.Price,
                Quantity = entry.Quantity,
            };
        }

        private OrderbookPriceEntry FillEmptyBuyOrderbookPriceEntry(OrderBook orderBook)
        {
            return new OrderbookPriceEntry()
            {
                ClientPairId = orderBook.ClientPairId,
                OrderSide = OrderSide.Buy,
                Price = 0,
                Quantity = 0,
                UtcCatchAt = orderBook.CatchAt.UtcDateTime,
            };
        }

        private OrderbookPriceEntry FillEmptySellOrderbookPriceEntry(OrderBook orderBook)
        {
            return new OrderbookPriceEntry()
            {
                ClientPairId = orderBook.ClientPairId,
                OrderSide = OrderSide.Sell,
                Price = 0,
                Quantity = 0,
                UtcCatchAt = orderBook.CatchAt.UtcDateTime,
            };
        }
    }
}