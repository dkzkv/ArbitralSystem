using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.PublicMarketInfoService.Domain.Interfaces;
using ArbitralSystem.PublicMarketInfoService.Domain.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArbitralSystem.PublicMarketInfoService.Persistence.Repositories
{
    public class PairInfoBaseRepository : BaseRepository, IPairInfoRepository
    {
        public PairInfoBaseRepository(PublicMarketInfoBdContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
        }

        public async Task<PairInfo> GetAsync(int id, CancellationToken cancellationToken)
        {
            var pairInfo = await DbContext.PairInfos.FirstOrDefaultAsync(o => o.Id.Equals(id), cancellationToken);
            return Mapper.Map<PairInfo>(pairInfo);
        }

        public async Task<PairInfo> CreateAsync(PairInfo pairInfo, CancellationToken cancellationToken)
        {
            var existedPair = pairInfo.Id.HasValue ? await GetAsync(pairInfo.Id.Value, cancellationToken) : null;
            if (existedPair != null)
                throw new InvalidOperationException($"Can not create PairInfo .PairInfo with id {pairInfo.Id} already exist");
            
            var savedPair = DbContext.PairInfos.Add(Mapper.Map<Entities.PairInfo>(pairInfo));
            await DbContext.SaveChangesAsync(cancellationToken);
            return Mapper.Map<PairInfo>(await GetAsync(savedPair.Entity.Id, cancellationToken));
        }

        public async Task<IEnumerable<PairInfo>> CreateRangeAsync(PairInfo[] pairInfos, CancellationToken cancellationToken)
        {
            var createdPairInfos = new List<PairInfo>();
            using (var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    foreach (var pairInfo in pairInfos)
                    {
                        var createdPair = await CreateAsync(pairInfo, cancellationToken);
                        createdPairInfos.Add(createdPair);
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            return createdPairInfos;
        }

        public async Task UpdateAsync(PairInfo pairInfo, CancellationToken cancellationToken)
        {
            var existedPair = await DbContext.PairInfos.FirstOrDefaultAsync(o => o.Id.Equals(pairInfo.Id), cancellationToken);
            if (existedPair is null) throw new InvalidOperationException($"Can not update, PairInfo with id: {pairInfo.Id} not exist");

            existedPair.ExchangePairName = pairInfo.ExchangePairName;
            existedPair.QuoteCurrency = pairInfo.QuoteCurrency;
            existedPair.BaseCurrency = pairInfo.BaseCurrency;
            existedPair.UtcDelistedAt = pairInfo.DelistedAt?.UtcDateTime;

            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRangeAsync(PairInfo[] pairInfos, CancellationToken cancellationToken)
        {
            using (var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                try
                {
                    foreach (var pairInfo in pairInfos)
                    {
                        await UpdateAsync(pairInfo, cancellationToken);
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }


        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existedPair = await DbContext.PairInfos.FirstOrDefaultAsync(o => o.Id.Equals(id), cancellationToken);
            if (existedPair is null) throw new InvalidOperationException($"Can not delete PairInfo. PairInfo with id: {id} not exist");

            DbContext.PairInfos.Remove(existedPair);
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}