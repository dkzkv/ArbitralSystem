using System;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Domain.Distributers;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot.SpotData;
using CryptoExchange.Net.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Test
{
    [TestClass]
    public class DistributerStatusConverterTest
    {
        private readonly IDtoConverter _dtoConverter;

        public DistributerStatusConverterTest()
        {
            _dtoConverter = new CryptoExchangeConverter();
        }

        [DataTestMethod]
        [DataRow(OrderBookStatus.Connecting)]
        [DataRow(OrderBookStatus.Disconnected)]
        [DataRow(OrderBookStatus.Synced)]
        [DataRow(OrderBookStatus.Syncing)]
        public void ConvertOrderBookStatusToDistributerStatus(OrderBookStatus rawStatus)
        {
            var targetStatus = _dtoConverter.Convert<OrderBookStatus, DistributerSyncStatus>(rawStatus);

            Assert.AreEqual((int) rawStatus, (int) targetStatus);
        }
        
        [DataTestMethod]
        public void ConvertOrderBookStatusToDistributerStatus1()
        {
            var a = Binance.Net.Enums.OrderSide.Sell;
            var targetStatus = _dtoConverter.Convert<Binance.Net.Enums.OrderSide, Domain.MarketInfo.OrderSide>(a);
            
            
            var a1 = Binance.Net.Enums.OrderType.Market;
            var bb = _dtoConverter.Convert<Binance.Net.Enums.OrderType, Domain.MarketInfo.OrderType>(a1);
            
            var a12 = Binance.Net.Enums.OrderType.Limit;
            var bb1 = _dtoConverter.Convert<Binance.Net.Enums.OrderType, Domain.MarketInfo.OrderType>(a12);

            var order = new BinanceOrder()
            {
                Side = OrderSide.Sell,
                CreateTime = DateTime.Now
            };
            
            var bb11 = _dtoConverter.Convert<BinanceOrder, IOrder>(order);
        }
        
    }
}