using System.Linq;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.PublicMarketInfoService.Domain.Queries.QueryModels;
using ArbitralSystem.PublicMarketInfoService.v1.Models;
using ArbitralSystem.PublicMarketInfoService.v1.Models.Paging;
using AutoMapper;

namespace ArbitralSystem.PublicMarketInfoService.Mapping
{
    internal class DomainToServiceMappingProfile : Profile
    {
        public DomainToServiceMappingProfile()
        {
            CreateMap(typeof(ArbitralSystem.Common.Pagination.Page<>), typeof(Page<>));
            CreateMap<IPairInfo, PairInfo>();
            CreateMap<IUniquePairInfo, UniquePairInfo>();
            
            CreateMap<IPairInfoPolygon, PairInfoPolygon>()
                .ForMember(destination => destination.UnificatedPairs, o => o.MapFrom(source => source.PolygonPairs.Select(o=>o.UnificatedPairName)));
            
            CreateMap<Exchange, ExchangeInfo>()
                .ForMember(dest => dest.Name, o => o.MapFrom(src => src.ToString()))
                .ForMember(dest => dest.Id, o => o.MapFrom(src => src));
        }
    }
}