using ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models;
using ArbitralSystem.Distributor.Core.Models;
using AutoMapper;

namespace ArbitralSystem.Distributor.MQDistributor.MQManagerService.Mapping
{
    internal class ConnectorsToDomainProfile : Profile
    {
        public ConnectorsToDomainProfile()
        {
            CreateMap<IArbitralPairInfo, PairInfo>()
                .ConstructUsing(o => new PairInfo(o.PairId, o.Exchange, o.UnificatedPairName, o.ExchangePairName));
        }
    }
}