using System;
using System.Linq;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Connectors.CoinEx;
using ArbitralSystem.Connectors.Core;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test.PublicConnectors
{
    [TestClass]
    public class PublicConnectorIntegrationTest
    {
        private const int TimeDelta = 5;
        private IPublicConnectorFactory _publicConnectorFactory;

        [TestInitialize]
        public void Init()
        {
            var dtoConverter = new CryptoExchangeConverter();
            _publicConnectorFactory = new CryptoExPublicConnectorFactory(dtoConverter,new EmptyLogger());
        }

        [DataTestMethod]
        [DataRow(Exchange.Binance)]
        [DataRow(Exchange.Bittrex)]
        [DataRow(Exchange.Huobi)]
        [DataRow(Exchange.Kraken)]
        [DataRow(Exchange.Kucoin)]
        //[DataRow(Exchange.CoinEx)] Not supported 
        //[DataRow(Exchange.Bitmex)] Not supported 
        //[DataRow(Exchange.Bitfinex)]
        public async Task GetServerTimeTest(Exchange exchange)
        {
            //Arrange
            var connector = _publicConnectorFactory.GetInstance(exchange);
            var dateTimeNow = DateTime.UtcNow;
            var timeStampNow = TimeHelper.DateTimeToTimeStamp(dateTimeNow);
            
            //Act    
            var serverTime = await connector.GetServerTime();

            //Assert
            Assert.AreEqual(serverTime, timeStampNow, TimeDelta);
        }

        [DataTestMethod]
        [DataRow(Exchange.Binance)]
        /*[DataRow(Exchange.Bittrex)]
        [DataRow(Exchange.Huobi)]
        [DataRow(Exchange.Kraken)]
        [DataRow(Exchange.Kucoin)]
        [DataRow(Exchange.CoinEx)]
        [DataRow(Exchange.Bitmex)]
        [DataRow(Exchange.Bitfinex)]*/
        public async Task GetPairInfoTest(Exchange exchange)
        {
            //Arrange
            var connector = _publicConnectorFactory.GetInstance(exchange);
            
            //Act
            var pairInfos = (await connector.GetPairsInfo()).ToArray();

            //Assert
            if (!pairInfos.Any())
                throw new Exception($"Empty pair info list from {exchange}");

            var pairInfo = pairInfos.First();

            Assert.IsNotNull(pairInfo.BaseCurrency);
            Assert.IsNotNull(pairInfo.QuoteCurrency);
            Assert.IsNotNull(pairInfo.ExchangePairName);
            Assert.IsNotNull(pairInfo.UnificatedPairName);
        }
        
        [DataTestMethod]
        [DataRow(Exchange.Binance)]
        [DataRow(Exchange.Bittrex)]
        [DataRow(Exchange.Huobi)]
        [DataRow(Exchange.Kraken)]
        [DataRow(Exchange.Kucoin)]
        [DataRow(Exchange.CoinEx)]
        [DataRow(Exchange.Bitmex)]
        [DataRow(Exchange.Bitfinex)]
        public async Task GetPairPricesTest(Exchange exchange)
        {
            //Arrange
            var connector = _publicConnectorFactory.GetInstance(exchange);
            
            //Act
            var prices = (await connector.GetPairPrices()).ToArray();

            //Assert
            Assert.IsTrue(prices.Any(o=>o.Price != null));
            Assert.IsTrue(prices.Any(o=>o.Exchange == exchange));
        }
    }
}