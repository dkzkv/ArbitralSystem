using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models;
using AutoMapper;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Persistence.Mapping
{
    public class PersistenceToDomain : Profile
    {
        public PersistenceToDomain()
        {
            CreateMap<Entities.DistributorState, DistributerState>()
                .ConstructUsing(o => new DistributerState(o.ClientPairId, o.UtcChangedAt, o.PreviousStatus, o.CurrentStatus));
        }
    }
}