using System.Collections.Generic;
using ArbitralSystem.Distributor.Core.Models;
using ArbitralSystem.Messaging.Messages;
using AutoMapper;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Common.Mapping
{
    internal class MessagesToServiceProfile : Profile
    {
        public MessagesToServiceProfile()
        {
            CreateMap<ArbitralSystem.Messaging.Models.PairInfo, PairInfo>()
                .ConstructUsing(o=> new PairInfo(o.PairId,o.Exchange,o.UnificatedPairName,o.ExchangePairName));
            
            CreateMap<IStartOrderBookDistribution, ExchangePairInfo>().ConstructUsing((o,ctx) => 
                new ExchangePairInfo(ctx.Mapper.Map<IEnumerable<PairInfo>>(o.PairInfos) ));
        }
    }
}