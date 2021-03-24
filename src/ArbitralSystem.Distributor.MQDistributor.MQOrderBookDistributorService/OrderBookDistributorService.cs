using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Jobs;
using JetBrains.Annotations;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService
{
    [UsedImplicitly]
    public class OrderBookDistributorService : IHostedService
    {
        private readonly IBusControl _busControl;
        private readonly JobManager _jobManager;
        private readonly ILogger _logger;
        
        public OrderBookDistributorService(IBusControl busControl,JobManager jobManager,
            ILogger logger)
        {
            _jobManager = jobManager;
            _busControl = busControl;
            _logger = logger;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _busControl.StartAsync(cancellationToken);
            await _jobManager.ActivateManager();
            _logger.Information("Service and messaging started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Cancellation started.");
            
            await _jobManager.DiActivateManager(cancellationToken, false);
            _logger.Information("Server diActivated without cancellation jobs.");
            
            await _busControl.StopAsync(cancellationToken);
            _logger.Information("Messaging stopped.");
            
            await _jobManager.DisposeAsync();
            _logger.Information("Job manager disposed.");
            
            _logger.Information("Service stopped.");
        }

    }
}