using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Trading.SimpleTradingBot.Common;


namespace ArbitralSystem.Trading.SimpleTradingBot.Strategies
{
    public interface IOrderGeneratorStrategy
    {
        Task InitializeAsync(IPairInfo firstPairInfo, IPairInfo secondPairInfo, CancellationToken token);
        bool Execute(ExchangesContext generatorContext, out (IPlaceOrder FirstOrder, IPlaceOrder SecondOrder) result);
    }
}