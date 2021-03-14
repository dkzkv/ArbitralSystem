using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.PublicMarketInfoService.Domain.Models;

namespace ArbitralSystem.PublicMarketInfoService.Domain.Interfaces
{
    public interface IPairInfoRepository
    {
        Task<PairInfo> GetAsync(int id, CancellationToken cancellationToken);
        Task<PairInfo> CreateAsync(PairInfo pairInfo, CancellationToken cancellationToken);
        Task<IEnumerable<PairInfo>> CreateRangeAsync(PairInfo[] pairInfos, CancellationToken cancellationToken);
        Task UpdateAsync(PairInfo pairInfo, CancellationToken cancellationToken);
        Task UpdateRangeAsync(PairInfo[] pairInfos, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}