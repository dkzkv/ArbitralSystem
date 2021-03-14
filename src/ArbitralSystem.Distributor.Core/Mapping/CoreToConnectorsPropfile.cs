using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Distributor.Core.Models;
using AutoMapper;

namespace ArbitralSystem.Distributor.Core.Mapping
{
    public class CoreToConnectorsProfile : Profile
    {
        public CoreToConnectorsProfile()
        {
            CreateMap<PairInfo, OrderBookPairInfo>().ConstructUsing(o => new OrderBookPairInfo(o.Exchange, o.ExchangePairName, o.UnificatedPairName, o.PairId));
        }
    }
}