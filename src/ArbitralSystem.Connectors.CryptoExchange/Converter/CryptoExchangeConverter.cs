using System.Collections.Generic;
using ArbitralSystem.Connectors.CoinEx.Models;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Domain.Distributers;
using ArbitralSystem.Domain.MarketInfo;
using AutoMapper;
using Binance.Net.Objects.Spot.MarketData;
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
                cfg.CreateMap<SymbolOrderBook, OrderBook>()
                    .ForMember(destination => destination.CatchAt, o => o.MapFrom(source => source.LastOrderBookUpdate.ToUniversalTime())); // LastOrderBookUpdate is utc date
                    //.AfterMap((src, dest) => dest.TimeStamp = TimeHelper.DateTimeToTimeStamp(dest.DateTime));

                #region Binance
                cfg.CreateMap<BinanceSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Name))
                    .ForMember(destination => destination.BaseCurrency, o => o.MapFrom(source => source.BaseAsset))
                    .ForMember(destination => destination.QuoteCurrency, o => o.MapFrom(source => source.QuoteAsset))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);
                
                
                cfg.CreateMap<BinancePrice, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Price))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Binance);
                #endregion

                #region Bittrex
                cfg.CreateMap<BittrexSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bittrex);
                
                cfg.CreateMap<BittrexSymbolSummary, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Ask))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Bittrex);
                #endregion

                #region Huobi
                cfg.CreateMap<HuobiSymbol, PairInfo>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Huobi);
                
                cfg.CreateMap<HuobiSymbolTick, PairPrice>()
                    .ForMember(destination => destination.ExchangePairName, o => o.MapFrom(source => source.Symbol))
                    .ForMember(destination => destination.Price, o => o.MapFrom(source => source.Ask))
                    .AfterMap((src, dest) => dest.Exchange = Exchange.Huobi);
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