using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Common.Settings;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Messaging.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore.Internal;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Jobs
{
    public class JobManager : IAsyncDisposable
    {
        private readonly DistributorServerSettings _serverSettings;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger _logger;
        private const ServerType ServerType = ArbitralSystem.Messaging.Models.ServerType.OrderBookDistributor;

        private Timer _heartBeatTimer;
        private ConcurrentDictionary<HeartBeatKey, DateTimeOffset> _heartBeatHistory;
        private long _lastHeartBeatCounter;
        private long _currentHeartBeatCounter;
        private Dictionary<Guid, CancellationTokenSource> _jobCancelManager;
        private Guid? _serverIdentifier;

        public JobManager(DistributorServerSettings serverSettings,
            IPublishEndpoint publishEndpoint,
            ILogger logger)
        {
            Preconditions.CheckNotNull(serverSettings, publishEndpoint, logger);
            _serverSettings = serverSettings;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task ActivateManager()
        {
            _serverIdentifier = Guid.NewGuid();
            _heartBeatHistory = new ConcurrentDictionary<HeartBeatKey, DateTimeOffset>();
            _jobCancelManager = new Dictionary<Guid, CancellationTokenSource>();
            await _publishEndpoint.Publish(
                new ServerCreatedMessage(_serverIdentifier.Value, _serverSettings.ServerName, ServerType, _serverSettings.MaxWorkersCount, DateTimeOffset.Now));

            _heartBeatTimer = new Timer(async (obj) => { await NotifyHeartBeat(); }, null, TimeSpan.FromSeconds(_serverSettings.HeartBeatIntervalInSeconds),
                TimeSpan.FromSeconds(_serverSettings.HeartBeatIntervalInSeconds));
        }

        public async Task AddAndExecuteInfiniteJob(Guid distributorId, Func<Task> func, CancellationTokenSource tokenSource)
        {
            IsManagerActivated();
            if (!_jobCancelManager.ContainsKey(distributorId))
            {
                _jobCancelManager.Add(distributorId, tokenSource);
                await NotifyDistributorActivated(distributorId);
                await ExecuteInfiniteJob(func, distributorId, tokenSource.Token);
            }
            else
            {
                _jobCancelManager[distributorId] = tokenSource;
            }
        }

        public bool IsExist(Guid distributorId)
        {
            IsManagerActivated();
            return _jobCancelManager.ContainsKey(distributorId);
        }

        public async Task CancelJob(Guid distributorId)
        {
            IsManagerActivated();
            if (_jobCancelManager.ContainsKey(distributorId))
            {
                _jobCancelManager[distributorId].Cancel();
                _jobCancelManager.Remove(distributorId);
                await NotifyDistributorDiActivated(distributorId);
            }

            foreach (var key in _heartBeatHistory.Keys.Where(o => o.DistributorId == distributorId))
            {
                _heartBeatHistory.Remove(key, out _);
            }
        }

        public async Task CancelAll(bool isSilentCancellation = true)
        {
            IsManagerActivated();
            foreach (var jobInfo in _jobCancelManager)
            {
                jobInfo.Value.Cancel();
                if(!isSilentCancellation)
                    await NotifyDistributorDiActivated(jobInfo.Key);
            }
        }

        public void HeartBeat(Guid botId, Exchange exchange, DateTimeOffset dateTimeOffset)
        {
            var key = new HeartBeatKey(botId, exchange);
            _heartBeatHistory.AddOrUpdate(key, dateTimeOffset, (oldKey, oldDatetimeOffset) =>
            {
                _currentHeartBeatCounter++;
                return dateTimeOffset;
            });
        }

        public async Task DiActivateManager(CancellationToken token, bool diActivateWithCancellation = true)
        {
            if (_serverIdentifier.HasValue)
            {
                await _publishEndpoint.Publish(new ServerDeletedMessage(_serverIdentifier.Value),token);
                if(diActivateWithCancellation)
                    await CancelAll(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_serverIdentifier.HasValue)
            {
                await _publishEndpoint.Publish(new ServerDeletedMessage(_serverIdentifier.Value));
                await CancelAll();
            }

            _heartBeatTimer?.Dispose();
            _serverIdentifier = null;
            _jobCancelManager?.Clear();
            _heartBeatHistory?.Clear();
        }

        private void IsManagerActivated()
        {
            if (!_serverIdentifier.HasValue)
                throw new InvalidOperationException("Manager is not activated.");
        }

        private async Task NotifyDistributorActivated(Guid distributorId)
        {
            if (_serverIdentifier.HasValue)
                await _publishEndpoint.Publish(
                    new OrderBookDistributorStatusMessage(distributorId, DistributorStatus.Activated, _serverIdentifier.Value));
        }

        private async Task NotifyDistributorDiActivated(Guid distributorId)
        {
            if (_serverIdentifier.HasValue)
                await _publishEndpoint.Publish(
                    new OrderBookDistributorStatusMessage(distributorId, DistributorStatus.DiActivated, _serverIdentifier.Value));
        }

        private async Task NotifyHeartBeat()
        {
            if (_lastHeartBeatCounter != _currentHeartBeatCounter)
            {
                _lastHeartBeatCounter = _currentHeartBeatCounter;
                var heartBeats = _heartBeatHistory.Select(o => new HeartBeatOrderBookDistributor()
                {
                    DateTimeOffset = o.Value,
                    DistributorId = o.Key.DistributorId,
                    Exchange = o.Key.Exchange
                }).ToArray();
                
                if (_serverIdentifier.HasValue && heartBeats.Any())
                    await _publishEndpoint.Publish(new HeartBeatOrderBookDistributorMessage(heartBeats));
            }
        }

        private async Task ExecuteInfiniteJob(Func<Task> func, Guid id, CancellationToken token)
        {
            int errorCounter = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await func.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Job: {id} finished with error, error count: {++errorCounter}");
                }
            }
        }
    }
}