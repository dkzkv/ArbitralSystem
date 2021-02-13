using System.Collections.Generic;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;

namespace ArbitralSystem.Connectors.Core.Types
{
    public delegate void OrderDelegate(IOrder order);
    public delegate void BalanceDelegate(IEnumerable<IBalance> balance);
    public delegate void OrderBookDelegate(IOrderBook orderBook);
    public delegate void DistributerStateDelegate(IDistributerState state);
}