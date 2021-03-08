using System.Collections.Generic;
using ArbitralSystem.Messaging.Messages;
using ArbitralSystem.Storage.MarketInfoStorageService.Domain.Models;
using AutoMapper;

namespace ArbitralSystem.Storage.MarketInfoStorageService.Mapping
{
    public class MessagesToDomainProfile : Profile
    {
        public MessagesToDomainProfile()
        {
            CreateMap<Messaging.Models.OrderbookEntry, OrderbookEntry>()
                .ConstructUsing(o=> new OrderbookEntry(o.Price,o.Quantity));

            CreateMap<IOrderBookMessage, OrderBook>()
                .ConstructUsing((o, ctx) => new OrderBook(o.ClientPairId,
                    o.CatchAt,
                    ctx.Mapper.Map<IEnumerable<OrderbookEntry>>(o.Asks),
                    ctx.Mapper.Map<IEnumerable<OrderbookEntry>>(o.Bids)));
        }
        
    }
}