using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models.Account;

namespace ArbitralSystem.Connectors.Core.PrivateConnectors
{
    public interface IExtraConnector
    {
        Task<IPairCommission> GetFeeRateAsync(string exchangePairName, CancellationToken token = default);
        Task<IEnumerable<IWithdrawCurrencyCommission>> GetWithdrawCommissionAsync(string currency, CancellationToken token = default);
    }
}