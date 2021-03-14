using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Messaging;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Messaging.Models;
using AutoMapper;

namespace ArbitralSystem.Distributor.MQDistributor.MQOrderBookDistributorService.Common.Mapping
{
    internal class ServiceToMessagesProfile : Profile
    {
        public ServiceToMessagesProfile()
        {
            CreateMap<IDistributorOrderBook,IOrderBookMessage>().As<OrderBookMessage>();
            CreateMap<IDistributorOrderBook,OrderBookMessage>();
            CreateMap<IOrderbookEntry, OrderbookEntry>();
            
            CreateMap<IDistributerState,IDistributerStateMessage>().As<DistributerStateMessage>();
            CreateMap<IDistributerState, DistributerStateMessage>();
        }
    }
}