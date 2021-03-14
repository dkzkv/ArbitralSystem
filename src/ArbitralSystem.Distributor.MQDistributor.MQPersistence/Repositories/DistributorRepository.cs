using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Interfaces;
using ArbitralSystem.Distributor.MQDistributor.MQPersistence.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Exchange = ArbitralSystem.Domain.MarketInfo.Exchange;

namespace ArbitralSystem.Distributor.MQDistributor.MQPersistence.Repositories
{
    public class DistributorRepository : IDistributorRepository
    {
        private readonly DistributorDbContext _ctx;
        private readonly IMapper _mapper;

        public DistributorRepository(DistributorDbContext ctx, IMapper mapper)
        {
            Preconditions.CheckNotNull(ctx, mapper);
            _mapper = mapper;
            _ctx = ctx;
        }

        public async Task<MQDomain.Models.Distributor> GetAsync(Guid id, CancellationToken token)
        {
            var result = await _ctx.Distributors
                .Include(o => o.Server)
                .Include(o => o.Exchanges).ThenInclude(o => o.Exchange)
                .FirstOrDefaultAsync(o => o.Id.Equals(id), token);

            try
            {
                return _mapper.Map<MQDomain.Models.Distributor>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public async Task<MQDomain.Models.Distributor> CreateAsync(MQDomain.Models.Distributor distributor, CancellationToken token)
        {
            var existedDistributor = await GetAsync(distributor.Id, token);
            if (existedDistributor != null)
                throw new InvalidOperationException("Distributor with same id already exist!");

            var distributorEntry = _ctx.Distributors.Add(_mapper.Map<Entities.Distributor>(distributor));

            foreach (var exchange in distributor.Exchanges)
            {
                var exchangeEntry = await GetExchangeEntityAndCreateIfNotExist(exchange, token);
                _ctx.DistributorExchanges.Add(new DistributorExchange()
                {
                    DistributorId = distributorEntry.Entity.Id,
                    ExchangeId = exchangeEntry.Id
                });
            }

            await _ctx.SaveChangesAsync(token);
            return await GetAsync(distributor.Id, token);
        }

        public async Task UpdateAsync(MQDomain.Models.Distributor distributor, CancellationToken token)
        {
            var existedDistributor = await _ctx.Distributors.FirstOrDefaultAsync(o => o.Id.Equals(distributor.Id), token);
            if (existedDistributor == null)
                throw new InvalidOperationException("Distributor with same id not exist!, Can't update");

            if (existedDistributor.ServerId != distributor?.Server.Id)
            {
                existedDistributor.ServerId = distributor?.Server.Id;
                existedDistributor.Server = _mapper.Map<Entities.Server>(distributor?.Server);
            }

            existedDistributor.Status = _mapper.Map<Entities.Status>(distributor?.Status);
            existedDistributor.Type = _mapper.Map<Entities.DistributorType>(distributor?.Type);
            existedDistributor.ModifyAt = distributor?.ModifyAt;

            await _ctx.SaveChangesAsync(token);
        }

        public async Task DeleteAsync(Guid id, CancellationToken token)
        {
            var existedPair = await _ctx.Distributors.FirstOrDefaultAsync(o => o.Id.Equals(id), token);
            if (existedPair is null) throw new InvalidOperationException($"Can not delete Distributor. Distributor with id: {id} not exist");

            _ctx.Distributors.Remove(existedPair);
            await _ctx.SaveChangesAsync(token);
        }

        private async Task<Entities.Exchange> GetExchangeEntityAndCreateIfNotExist(Exchange exchange, CancellationToken cancellationToken)
        {
            var exchangeEntry = await _ctx.Exchanges.FirstOrDefaultAsync(o => o.Id == (int) exchange, cancellationToken);
            if (exchangeEntry == null)
            {
                _ctx.Exchanges.Add(new Entities.Exchange()
                {
                    Id = (int) exchange,
                    Name = exchange.ToString()
                });
                await _ctx.SaveChangesAsync(cancellationToken);
                exchangeEntry = await _ctx.Exchanges.FirstOrDefaultAsync(o => o.Id == (int) exchange, cancellationToken);
            }

            return exchangeEntry;
        }
    }
}