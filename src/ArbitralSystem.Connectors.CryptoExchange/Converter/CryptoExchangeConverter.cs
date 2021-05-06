using System.Collections.Generic;
using System.Linq;
using ArbitralSystem.Connectors.CoinEx.Models;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;
using AutoMapper;
using AutoMapper.Extensions.EnumMapping;
using Binance.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Objects.Spot.SpotData;
using Bitfinex.Net.Objects;
using Bitmex.Net.Client.Objects;
using Bittrex.Net.Objects;
using CoinEx.Net.Objects;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.OrderBook;
using Huobi.Net.Objects;
using Kraken.Net.Objects;
using Kucoin.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.Converter
{
    public class CryptoExchangeConverter : IDtoConverter
    {
        private readonly IMapper _mapper;

        public CryptoExchangeConverter()
        {
            var config = CreateMappingConfiguration();
            _mapper = config.CreateMapper();
        }

        public TDestination Convert<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        private MapperConfiguration CreateMappingConfiguration()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OrderBookStatus, DistributerSyncStatus>();

                cfg.CreateMap<ISymbolOrderBookEntry, IOrderbookEntry>().As<OrderbookEntry>();
                cfg.CreateMap<ISymbolOrderBookEntry, OrderbookEntry>();
                cfg.CreateMap<SymbolOrderBook, DistributorOrderBook>()
                    .ForMember(destination => destination.CatchAt, o => o.MapFrom(source => source.LastOrderBookUpdate.ToUniversalTime())); // LastOrderBookUpdate is utc date
                    //.AfterMap((src, dest) => dest.TimeStamp = TimeHelper.DateTimeToTimeStamp(dest.DateTime));

                #region Binance

                cfg.CreateMap<BinanceOrderBookEntry, IOrderbookEntry>().As<OrderbookEntry>();;
                cfg.CreateMap<BinanceOrderBookEntry, OrderbookEntry>();
                
                cfg.CreateMap<BinanceOrderBook, IOrderBook>().As<OrderBook>();;
                cfg.CreateMap<BinanceOrderBook, OrderBook>()
                    .ForMember(destination => destination.BestAsk, o => o.MapFrom(source => source.Asks.FirstOrDefault()))
                    .ForMember(destination => destination.BestBid, o => o.MapFrom(source => source.Bids.FirstOrDefault()))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);
                
                cfg.CreateMap<BinanceSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Name))
                    .ForMember(destination => destination.BaseCurrency, o => o.MapFrom(source => source.BaseAsset))
                    .ForMember(destination => destination.QuoteCurrency, o => o.MapFrom(source => source.QuoteAsset))
                    //TODO HZ
                    .ForMember(destination => destination.AmountPrecision, o => o.MapFrom(source => source.BaseAssetPrecision))
                    
                    .ForMember(destination => destination.MinMarketOrderAmount, o => o.MapFrom(source => source.MarketLotSizeFilter.MinQuantity))
                    .ForMember(destination => destination.MaxMarketOrderAmount, o => o.MapFrom(source => source.MarketLotSizeFilter.MinQuantity))
                    
                    .ForMember(destination => destination.MinLimitOrderAmount, o => o.MapFrom(source => source.LotSizeFilter.MinQuantity))
                    .ForMember(destination => destination.MaxLimitOrderAmount, o => o.MapFrom(source => source.LotSizeFilter.MinQuantity))
                    
                    .ForMember(destination => destination.MinLimitOrderValue, o => o.MapFrom(source => source.MinNotionalFilter.MinNotional))
                    .ForMember(destination => destination.MaxLimitOrderAmount, o => o.Ignore())
                    
                    .ForMember(destination => destination.MinMarketOrderValue, o => o.MapFrom(source => source.MinNotionalFilter.ApplyToMarketOrders ?
                        source.MinNotionalFilter.MinNotional : (decimal?)null))
                    .ForMember(destination => destination.MaxLimitOrderAmount, o => o.Ignore())
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);
                
                
                cfg.CreateMap<BinancePrice, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Price))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);
                
                cfg.CreateMap<BinanceBalance, IBalance>().As<Balance>();
                cfg.CreateMap<BinanceBalance, Balance>()
                    .ForMember(destination => destination.Currency, o => o.MapFrom(source => source.Asset))
                    .ForMember(destination => destination.Total, o => o.MapFrom(source => source.Total))
                    .ForMember(destination => destination.Available, o => o.MapFrom(source => source.Free))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);
                
                cfg.CreateMap<BinanceOrder, IOrder>().As<Models.Auxiliary.Order>();
                cfg.CreateMap<BinanceOrder, Models.Auxiliary.Order>()
                    .ForMember(destination => destination.Id, o => o.MapFrom(source => source.OrderId.ToString()))
                    .ForMember(destination => destination.ClientOrderId, o => o.MapFrom(source => source.ClientOrderId))
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.OrderSide,  o => o.MapFrom(source => source.Side))
                    .ForMember(destination => destination.OrderType, o => o.MapFrom(source => source.Type))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Price))
                    .ForMember(destination => destination.Quantity, o => o.MapFrom(source => source.Quantity))
                    .ForMember(destination => destination.IsActive, o => o.MapFrom(source => source.Status))
                    .ForMember(destination => destination.CreatedAt, o => o.MapFrom(source => source.CreateTime))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);

                cfg.CreateMap<Binance.Net.Enums.OrderStatus, bool>()
                    .ConstructUsing(o => o == Binance.Net.Enums.OrderStatus.New ||
                                         o == Binance.Net.Enums.OrderStatus.PartiallyFilled );
                
                cfg.CreateMap<Binance.Net.Enums.OrderSide, Domain.MarketInfo.OrderSide>()
                    .ConvertUsingEnumMapping(opt => opt
                        .MapByName()
                    ).ReverseMap();

                cfg.CreateMap<Binance.Net.Enums.OrderType, Domain.MarketInfo.OrderType>()
                    .ConvertUsingEnumMapping(opt => opt
                        .MapByName()
                    ).ReverseMap();
                
                #endregion

                #region Bittrex
                cfg.CreateMap<BittrexSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bittrex);
                
                cfg.CreateMap<BittrexSymbolSummary, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.High))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bittrex);
                #endregion

                #region Huobi
                
                cfg.CreateMap<HuobiOrderBookEntry, IOrderbookEntry>().As<OrderbookEntry>();;
                cfg.CreateMap<HuobiOrderBookEntry, OrderbookEntry>();
                
                cfg.CreateMap<HuobiOrderBook, IOrderBook>().As<OrderBook>();;
                cfg.CreateMap<HuobiOrderBook, OrderBook>()
                    .ForMember(destination => destination.BestAsk, o => o.MapFrom(source => source.Asks.FirstOrDefault()))
                    .ForMember(destination => destination.BestBid, o => o.MapFrom(source => source.Bids.FirstOrDefault()))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Huobi);
                
                cfg.CreateMap<HuobiSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    
                    .ForMember(destination => destination.AmountPrecision, o => o.MapFrom(source => source.AmountPrecision))
                    
                    .ForMember(destination => destination.MinMarketOrderAmount, o => o.MapFrom(source => source.MinMarketSellOrderAmount))
                    .ForMember(destination => destination.MaxMarketOrderAmount, o => o.MapFrom(source => source.MaxMarketSellOrderAmount))
                    
                    .ForMember(destination => destination.MinLimitOrderAmount, o => o.MapFrom(source => source.MinLimitOrderAmount))
                    .ForMember(destination => destination.MaxLimitOrderAmount, o => o.MapFrom(source => source.MaxLimitOrderAmount))
                    
                    .ForMember(destination => destination.MinLimitOrderValue, o => o.MapFrom(source => source.MinOrderValue))
                    .ForMember(destination => destination.MaxLimitOrderAmount, o => o.Ignore())
                    
                    .ForMember(destination => destination.MinMarketOrderValue, o => o.MapFrom(source => source.MinOrderValue))
                    .ForMember(destination => destination.MaxLimitOrderAmount, o => o.Ignore())
                    
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Huobi);
                
                cfg.CreateMap<HuobiSymbolTick, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Ask))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Huobi);
                
                cfg.CreateMap<HuobiOrder, IOrder>().As<Models.Auxiliary.Order>();
                cfg.CreateMap<HuobiOrder, Models.Auxiliary.Order>()
                    .ForMember(destination => destination.Id, o => o.MapFrom(source => source.Id.ToString()))
                    .ForMember(destination => destination.ClientOrderId, o => o.MapFrom(source => source.ClientOrderId))
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.OrderSide,  o => o.MapFrom(source => source.Type))
                    .ForMember(destination => destination.OrderType, o => o.MapFrom(source => source.Type))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Price))
                    .ForMember(destination => destination.Quantity, o => o.MapFrom(source => source.Amount))
                    .ForMember(destination => destination.IsActive, o => o.MapFrom(source => source.State))
                    .ForMember(destination => destination.CreatedAt, o => o.MapFrom(source => source.CreatedAt))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Huobi);
                
                cfg.CreateMap<HuobiOrderState, bool>()
                    .ConstructUsing(o => o == HuobiOrderState.Created || 
                                         o == HuobiOrderState.PreSubmitted ||
                                         o == HuobiOrderState.Submitted ||
                                         o == HuobiOrderState.PartiallyFilled);
                
                cfg.CreateMap<HuobiOrderType, Domain.MarketInfo.OrderSide>()
                    .ConvertUsingEnumMapping(opt => opt
                        .MapValue(HuobiOrderType.LimitBuy,  Domain.MarketInfo.OrderSide.Buy)
                        .MapValue(HuobiOrderType.LimitSell,  Domain.MarketInfo.OrderSide.Sell)
                        .MapValue(HuobiOrderType.MarketBuy,  Domain.MarketInfo.OrderSide.Buy)
                        .MapValue(HuobiOrderType.MarketSell,  Domain.MarketInfo.OrderSide.Sell)
                    ).ReverseMap();

                cfg.CreateMap<HuobiOrderType, Domain.MarketInfo.OrderType>()
                    .ConvertUsingEnumMapping(opt => opt
                        .MapValue(HuobiOrderType.LimitBuy,  Domain.MarketInfo.OrderType.Limit)
                        .MapValue(HuobiOrderType.LimitSell,  Domain.MarketInfo.OrderType.Limit)
                        .MapValue(HuobiOrderType.MarketBuy,  Domain.MarketInfo.OrderType.Market)
                        .MapValue(HuobiOrderType.MarketSell,  Domain.MarketInfo.OrderType.Market)
                    ).ReverseMap();
                #endregion

                #region Kraken
                cfg.CreateMap<KrakenSymbol, PairInfo>()
                    .ForMember(destination => destination.BaseCurrency, o => o.MapFrom(source => source.BaseAsset))
                    .ForMember(destination => destination.QuoteCurrency, o => o.MapFrom(source => source.QuoteAsset))
                    .ForMember(destination => destination.ExchangePairName,
                        o => o.MapFrom(source => source.WebsocketName))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Kraken);
                
                cfg.CreateMap<KeyValuePair<string,KrakenRestTick>, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Key))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Value.BestAsks.Price))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Kraken);
                #endregion
                
                #region Kucoin
                cfg.CreateMap<KucoinSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Kucoin);
                
                cfg.CreateMap<KucoinAllTick, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.BestAsk))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Kucoin);
                #endregion

                #region CoinEx
                cfg.CreateMap<IMarketInfo, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Name))
                    .ForMember(destination => destination.BaseCurrency, o => o.MapFrom(source => source.TradingName))
                    .ForMember(destination => destination.QuoteCurrency, o => o.MapFrom(source => source.PricingName))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.CoinEx);
                
                cfg.CreateMap<KeyValuePair<string, CoinExSymbolStateData>, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Key))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Value.BestBuyPrice))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.CoinEx);
                #endregion

                #region Bitmex
                cfg.CreateMap<Instrument, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.BaseCurrency, o => o.MapFrom(source => source.RootSymbol))
                    .ForMember(destination => destination.QuoteCurrency, o => o.MapFrom(source => source.QuoteCurrency))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bitmex);
                
                cfg.CreateMap<Instrument, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.AskPrice))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bitmex);
                #endregion

                #region Bifinex
                cfg.CreateMap<BitfinexSymbolOverview, IPairPrice>().As<PairPrice>();
                cfg.CreateMap<BitfinexSymbolOverview, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Ask))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bitfinex);
                #endregion
            });
        }
    }
}