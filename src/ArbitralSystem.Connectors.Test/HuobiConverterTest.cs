using System;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models.Trading;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Domain.MarketInfo;
using AutoMapper;
using Huobi.Net.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Test
{
    [TestClass]
    public class HuobiConverterTest
    {
        private IDtoConverter _dtoConverter;
        
        [TestInitialize]
        public void Init()
        {
            _dtoConverter = new CryptoExchangeConverter();
        }
        
        [DataTestMethod]
        [DataRow(HuobiOrderType.LimitBuy, OrderType.Limit)]
        [DataRow(HuobiOrderType.LimitSell, OrderType.Limit)]
        [DataRow(HuobiOrderType.MarketBuy, OrderType.Market)]
        [DataRow(HuobiOrderType.MarketSell, OrderType.Market)]
        public void Convert_Huobi_OrderType_To_Arbitral(HuobiOrderType exchangeOrderType, OrderType targetType )
        {
            //Act
            var orderType = _dtoConverter.Convert<HuobiOrderType, OrderType>(exchangeOrderType);
            
            //Assert
            Assert.AreEqual(orderType,targetType);
        }
        
        [DataTestMethod]
        [DataRow(HuobiOrderType.LimitBuy, OrderSide.Buy)]
        [DataRow(HuobiOrderType.LimitSell, OrderSide.Sell)]
        [DataRow(HuobiOrderType.MarketBuy, OrderSide.Buy)]
        [DataRow(HuobiOrderType.MarketSell, OrderSide.Sell)]
        public void Convert_Huobi_OrderSide_To_Arbitral(HuobiOrderType exchangeOrderSide, OrderSide targetSide )
        {
            //Act
            var orderSide = _dtoConverter.Convert<HuobiOrderType, OrderSide>(exchangeOrderSide);
            
            //Assert
            Assert.AreEqual(orderSide,targetSide);
        }
        
        [DataTestMethod]
        [ExpectedException(typeof(AutoMapperMappingException))]
        public void Convert_Huobi_OrderType_To_Arbitral_with_Exception( )
        {
            //Arrange
            var errorType = HuobiOrderType.FillOrKillLimitBuy;
            
            //Act
            var orderType = _dtoConverter.Convert<HuobiOrderType, Domain.MarketInfo.OrderType>(errorType);
        }
        
        [DataTestMethod]
        public void Convert_Huobi_PlaceOrder_To_Arbitral( )
        {
            //Arrange
            const string clientOrderId = "clientOrderId";
            const string symbol = "eth/btc";
            DateTime createdDate = DateTime.Now;
            const decimal price = 123;
            const decimal quantity = 321;
            const long orderId = 12345678;

            var huobiOrder = new HuobiOrder()
            {
                ClientOrderId = clientOrderId,
                Symbol = symbol,
                CreatedAt = createdDate,
                Price = price,
                Amount = quantity,
                Id = orderId,
                Type = HuobiOrderType.LimitSell,
                State = HuobiOrderState.Created
            };
            
            //Act
            var order = _dtoConverter.Convert<HuobiOrder, IOrder>(huobiOrder);
            
            //Assert
            Assert.AreEqual(order.ClientOrderId, clientOrderId);
            Assert.AreEqual(order.ExchangePairName, symbol);
            Assert.AreEqual(order.CreatedAt, createdDate);
            Assert.AreEqual(order.Price, price);
            Assert.AreEqual(order.Quantity, quantity);
            Assert.AreEqual(order.Id, orderId.ToString());
            Assert.AreEqual(order.OrderType, OrderType.Limit);
            Assert.AreEqual(order.OrderSide, OrderSide.Sell);
            Assert.AreEqual(order.IsActive, true);
            Assert.AreEqual(order.Exchange, Exchange.Huobi);
        }
    }
}