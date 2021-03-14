using System.Linq;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Common;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Models;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Queries.QueryModels;
using ArbitralSystem.Distributor.MQDistributor.MQPersistence.Mapping.AuxiliaryModels;
using ArbitralSystem.Domain.MarketInfo;
using AutoMapper;

namespace ArbitralSystem.Distributor.MQDistributor.MQPersistence.Mapping
{
    public class PersistenceToDomainProfile : Profile
    {
        public PersistenceToDomainProfile()
        {
            CreateMap<Entities.Server, Server>()
                .ConstructUsing((o,m) => new Server(o.Id,
                    o.Name,
                    m.Mapper.Map<ServerType>(o.ServerType),
                    o.MaxWorkersCount,
                    o.CreatedAt,
                    o.ModifyAt,
                    o.IsDeleted));
            
            CreateMap<Entities.Distributor, MQDomain.Models.Distributor>()
                .ConstructUsing((o,m) => new MQDomain.Models.Distributor(o.Id,
                    o.Name,
                    m.Mapper.Map<DistributorType>(o.Type),
                    o.CreatedAt,
                    o.ModifyAt,
                    m.Mapper.Map<Status>(o.Status),
                    m.Mapper.Map<Server>(o.Server),
                    o.Exchanges.Select(ex=> (Exchange)ex.ExchangeId)))
                .ForMember(x => x.Exchanges, opt => opt.Ignore());
            
            CreateMap<Entities.Server,IServer>().As<ServerAuxiliaryModel>();
            CreateMap<Entities.Server,ServerAuxiliaryModel>();
            
            CreateMap<Entities.Server,IServerReference>().As<ServerReferenceAuxiliaryModel>();
            CreateMap<Entities.Server,ServerReferenceAuxiliaryModel>();
            
            CreateMap<Entities.Distributor,IDistributor>().As<DistributorAuxiliaryModel>();
            CreateMap<Entities.Distributor,DistributorAuxiliaryModel>();
            
            CreateMap<Entities.Distributor,IOrderBookDistributor>().As<OrderBookDistributorAuxiliaryModel>();
            CreateMap<Entities.Distributor, OrderBookDistributorAuxiliaryModel>()
                .ForMember(destination => destination.ExchangeInfos,
                    o => o.MapFrom(source =>source.Exchanges.Select( ex => new ExchangeInfo((Exchange)ex.Exchange.Id, ex.HeartBeat))));
        }
    }
}