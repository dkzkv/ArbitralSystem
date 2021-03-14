using System;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Domain.MarketInfo;
using AutoMapper;
using Binance.Net.Objects.Spot.SpotData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderSide = Binance.Net.Enums.OrderSide;
using OrderStatus = Binance.Net.Enums.OrderStatus;
using OrderType = Binance.Net.Enums.OrderType;

namespace ArbitralSystem.Connectors.Test
{
    [TestClass]
    public class BinanceConverterTest
    {
        private IDtoConverter _dtoConverter;
        
        [TestInitialize]
        public void Init()
        {
            _dtoConverter = new CryptoExchangeConverter();
        }
        
        [DataTestMethod]
        [DataRow(Binance.Net.Enums.OrderType.Market, Domain.MarketInfo.OrderType.Market)]
        [DataRow(Binance.Net.Enums.OrderType.Limit, Domain.MarketInfo.OrderType.Limit)]
        public void Convert_Binance_OrderType_To_Arbitral(Binance.Net.Enums.OrderType exchangeOrderType, Domain.MarketInfo.OrderType targetType )
        {
            //Act
            var orderType = _dtoConverter.Convert<Binance.Net.Enums.OrderType, Domain.MarketInfo.OrderType>(exchangeOrderType);
            
            //Assert
            Assert.AreEqual(orderType,targetType);
        }
        
        [DataTestMethod]
        [DataRow(Binance.Net.Enums.OrderSide.Buy, Domain.MarketInfo.OrderSide.Buy)]
        [DataRow(Binance.Net.Enums.OrderSide.Sell, Domain.MarketInfo.OrderSide.Sell)]
        public void Convert_Binance_OrderSide_To_Arbitral(Binance.Net.Enums.OrderSide exchangeOrderSide, Domain.MarketInfo.OrderSide targetSide )
        {
            //Act
            var orderSide = _dtoConverter.Convert<Binance.Net.Enums.OrderSide, Domain.MarketInfo.OrderSide>(exchangeOrderSide);
            
            //Assert
            Assert.AreEqual(orderSide,targetSide);
        }
        
        [DataTestMethod]
        [ExpectedException(typeof(AutoMapperMappingException))]
        public void Convert_Binance_OrderSide_To_Arbitral_with_Exception( )
        {
            //Arrange
            var errorType = Binance.Net.Enums.OrderType.Liquidation;
            
            //Act
            var orderSide = _dtoConverter.Convert<Binance.Net.Enums.OrderType, Domain.MarketInfo.OrderType>(errorType);
        }
        
        [DataTestMethod]
        public void Convert_Binance_PlaceOrder_To_Arbitral( )
        {
            //Arrange
            const string clientOrderId = "clientOrderId";
            const string symbol = "eth/btc";
            DateTime createdDate = DateTime.Now;
            const decimal price = 123;
            const decimal quantity = 321;
            const long orderId = 12345678;
            const OrderSide side = OrderSide.Sell;
            const OrderType type = OrderType.Market;
            const OrderStatus status = OrderStatus.New;
                
            var binanceOrder = new BinanceOrder()
            {
                ClientOrderId = clientOrderId,
                Symbol = symbol,
                CreateTime = createdDate,
                Price = price,
                Quantity = quantity,
                OrderId = orderId,
                Type = type,
                Side = side,
                Status = status
            };
            
            //Act
            var order = _dtoConverter.Convert<BinanceOrder, IOrder>(binanceOrder);
            
            //Assert
            Assert.AreEqual(order.ClientOrderId, clientOrderId);
            Assert.AreEqual(order.ExchangePairName, symbol);
            Assert.AreEqual(order.CreatedAt, createdDate);
            Assert.AreEqual(order.Price, price);
            Assert.AreEqual(order.Quantity, quantity);
            Assert.AreEqual(order.Id, orderId.ToString());
            Assert.AreEqual(order.OrderType, Domain.MarketInfo.OrderType.Market);
            Assert.AreEqual(order.OrderSide, Domain.MarketInfo.OrderSide.Sell);
            Assert.AreEqual(order.IsActive, true);
            Assert.AreEqual(order.Exchange, Exchange.Binance);
        }
        
        [DataTestMethod]

        public void Convert_Binance_Balance_To_Arbitral( )
        {
            //Arrange
            const string currency = "ETH";
            const decimal total = 100;
            const decimal free = 99;
            var binanceBalance = new BinanceBalance()
            {
                Asset = currency,
                Free = free,
                Locked = total - free
            };

            //Act
            var balance = _dtoConverter.Convert<BinanceBalance, IBalance>(binanceBalance);
            
            //Assert
            Assert.AreEqual(balance.Currency, currency);
            Assert.AreEqual(balance.Total, total);
            Assert.AreEqual(balance.Available, free);
            Assert.AreEqual(balance.Exchange, Exchange.Binance);
        }
    }
}