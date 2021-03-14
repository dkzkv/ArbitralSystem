using System.Collections.Generic;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.Core.Types;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public abstract class BasePrivateSocketConnector
    {
        private event BalanceDelegate BalanceHandler;

        public event BalanceDelegate BalanceChanged
        {
            add => BalanceHandler += value;
            remove => BalanceHandler -= value;
        }

        protected void OnBalance(IEnumerable<IBalance> balance) => BalanceHandler?.Invoke(balance);

        public event OrderDelegate OrderHandler;

        public event OrderDelegate OrderChanged
        {
            add => OrderHandler += value;
            remove => OrderHandler -= value;
        }

        protected void OnOrder(IOrder order) => OrderHandler?.Invoke(order);
    }
}