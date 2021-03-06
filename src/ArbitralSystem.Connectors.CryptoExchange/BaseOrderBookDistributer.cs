﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Types;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.OrderBook;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange
{
    internal abstract class BaseOrderBookDistributer<TExchange> where TExchange : SymbolOrderBook
    {
        private readonly IConverter _converter;
        private readonly IDistributerOptions _distributerOptions;
        private readonly ILogger _logger;

        private OrderBookDistributerInstance<TExchange> _orderBookDistributerInstance;

        protected BaseOrderBookDistributer([NotNull] IDistributerOptions distributerOptions,
            [NotNull] IConverter converter,
            [NotNull] ILogger logger)
        {
            _distributerOptions = distributerOptions;
            _converter = converter;
            _logger = logger;
        }

        public abstract Exchange Exchange { get; }
        public virtual IDistributerState DistributerState { get; private set; }

        protected abstract TExchange CreateSymbolOrderBook(string symbol);

        public virtual async Task<Task> StartDistributionAsync(OrderBookPairInfo pairInfo, CancellationToken token)
        {
            var orderBook = CreateSymbolOrderBook(pairInfo.ExchangePairName);
            orderBook.OnStatusChange += OrderBook_OnStatusChange;

            _orderBookDistributerInstance = new OrderBookDistributerInstance<TExchange>
            {
                InstanceSymbol = pairInfo.UnificatedPairName,
                OrderBook = orderBook,
                Token = token
            };

            if (token.IsCancellationRequested)
            {
                orderBook.OnStatusChange -= OrderBook_OnStatusChange;
                return Task.CompletedTask;
            }

            var response = await orderBook.StartAsync().ConfigureAwait(false);

            if (!response.Success)
            {
                orderBook.OnStatusChange -= OrderBook_OnStatusChange;
                throw new WebsocketException(
                    $"Error while connecting to exchange {Exchange}:{pairInfo.UnificatedPairName}({pairInfo.ExchangePairName}): {response.Error.Message}");
            }

            return Task.Factory.StartNew(() => WatchOnOrderBook(_orderBookDistributerInstance),
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void OrderBook_OnStatusChange(OrderBookStatus arg1, OrderBookStatus arg2)
        {
            var currentStatus = _converter.Convert<OrderBookStatus, DistributerSyncStatus>(arg2);
            var previousStatus = _converter.Convert<OrderBookStatus, DistributerSyncStatus>(arg1);

            var state = new DistributerState
            {
                PreviousStatus = previousStatus,
                CurrentStatus = currentStatus,
                Exchange = Exchange,
                Symbol = _orderBookDistributerInstance.InstanceSymbol,
                ChangedAt = DateTimeOffset.UtcNow
            };
            OnStatusChanged(state);
        }

        private void WatchOnOrderBook(OrderBookDistributerInstance<TExchange> instance)
        {
            var firstOrderBook = FillOrderBook(instance);
            OnOrderBook(firstOrderBook);
            
            var updateNumber = instance.OrderBook.LastSequenceNumber;
            var updateTime = DateTimeOffset.Now;
            while (!instance.Token.IsCancellationRequested)
            {
                try
                {
                    if (updateNumber != instance.OrderBook.LastSequenceNumber)
                    {
                        updateTime = DateTimeOffset.Now;
                        if (!instance.OrderBook.Asks.Any() && !instance.OrderBook.Bids.Any())
                        {
                            _logger.Warning($"Empty orderbook received.");
                        }
                        
                        var orderBook = FillOrderBook(instance);
                        OnOrderBook(orderBook);
                        updateNumber = instance.OrderBook.LastSequenceNumber;
                    }
                    else
                    {
                        if ((DateTimeOffset.Now - updateTime) > TimeSpan.FromSeconds(_distributerOptions.SilenceLimitInSeconds))
                        {
                            _logger.Warning($"Orderbook in silence more than {_distributerOptions.SilenceLimitInSeconds}, reconnect initiated.");
                            instance.OrderBook.Stop();
                            _logger.Debug($"Orderbook behaviour stopped.");
                            Thread.Sleep(_distributerOptions.Frequency);
                            instance.OrderBook.Start();
                            _logger.Debug($"Orderbook behaviour started.");
                        }
                    }
                    Thread.Sleep(_distributerOptions.Frequency);
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex, "STRANGE ANOMALY while watching on orderbook changes");
                }
            }

            _logger.Information($"Distribution canceled: {instance.Token.IsCancellationRequested}, for pair: {instance.InstanceSymbol}, Exchange: {Exchange}");
            instance.OrderBook.Stop();
            instance.OrderBook.OnStatusChange -= OrderBook_OnStatusChange;
            instance.OrderBook.Dispose();
        }

        private IOrderBook FillOrderBook(OrderBookDistributerInstance<TExchange> instance)
        {
            var orderBook = _converter.Convert<SymbolOrderBook, OrderBook>(instance.OrderBook);
            orderBook.Symbol = instance.InstanceSymbol;
            orderBook.Exchange = Exchange;
            return orderBook;
        }

        private event OrderBookDelegate OrderBookHandler;

        public event OrderBookDelegate OrderBookChanged
        {
            add => OrderBookHandler += value;
            remove => OrderBookHandler -= value;
        }

        private event DistributerStateDelegate DistributerStatusHandler;

        public event DistributerStateDelegate DistributerStateChanged
        {
            add => DistributerStatusHandler += value;
            remove => DistributerStatusHandler -= value;
        }

        private void OnOrderBook(IOrderBook orderBook)
        {
            OrderBookHandler?.Invoke(orderBook);
        }

        private void OnStatusChanged(IDistributerState state)
        {
            DistributerStatusHandler?.Invoke(state);
        }
    }
}