using System;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Types;

namespace ArbitralSystem.Connectors.Core.PrivateConnectors
{
    public interface IPrivateSocketConnector : IDisposable
    {
        Task SubscribeToBalanceUpdatesAsync(CancellationToken ct = default);
        
        Task SubscribeToOrdersUpdatesAsync(string exchangePairName, CancellationToken ct = default);
        
        event BalanceDelegate BalanceChanged;

        event OrderDelegate OrderChanged;
    }
}