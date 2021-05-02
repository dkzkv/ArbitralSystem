using System.Linq;
using ArbitralSystem.Distributor.MQDistributor.MQDomain.Queries.QueryModels;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models.LookUps;
using ArbitralSystem.Distributor.MQDistributor.MQManagerService.v1.Models.Paging;
using AutoMapper;
using Status = ArbitralSystem.Distributor.MQDistributor.MQDomain.Common.Status;

namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.Mapping
{
    internal class DomainToApiProfile : Profile
    {
        public DomainToApiProfile()
        {
            CreateMap(typeof(ArbitralSystem.Common.Pagination.Page<>), typeof(Page<>));
            
            CreateMap<IExchangeInfo, ExchangeInfo>();
            
            CreateMap<IDistributor, FullDistributorResult>();
            CreateMap<IOrderBookDistributor, FullDistributorResult>()
                .IncludeAllDerived();
            
            CreateMap<IDistributor, DistributorResult>();
            CreateMap<IOrderBookDistributor, DistributorResult>()
                .IncludeAllDerived();
            
            CreateMap<IDistributor, FullDistributorWithServerInfoResult>()
                .ForMember(destination => destination.ServerId, o => o.MapFrom(source => source.Server.Id ));
            CreateMap<IOrderBookDistributor, FullDistributorWithServerInfoResult>()
                .IncludeAllDerived()
                .ForMember(destination => destination.ServerId, o => o.MapFrom(source => source.Server.Id));

            CreateMap<IServer, ServerResult>();
            CreateMap<IServer, ShortServerResult>()
                .IncludeAllDerived()
                .ForMember(destination => destination.ActiveWorkers, o => o.MapFrom(source => source.Distributors.Count(o => o.Status != Status.Deleted)));
            
            CreateMap<IServer, FullServerResult>()
                .IncludeAllDerived();

            CreateMap<ArbitralSystem.Distributor.MQDistributor.MQDomain.Common.ServerType, ServerTypeInfo>()
                .ForMember(dest => dest.Id, o => o.MapFrom(src => src))
                .ForMember(dest => dest.Name, o => o.MapFrom(src => src.ToString()));
            
            CreateMap<Status, StatusInfo>()
                .ForMember(dest => dest.Id, o => o.MapFrom(src => src))
                .ForMember(dest => dest.Name, o => o.MapFrom(src => src.ToString()));

        }
    }
}