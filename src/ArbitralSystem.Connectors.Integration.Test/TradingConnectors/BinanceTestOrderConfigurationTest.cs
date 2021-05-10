using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Connectors.CryptoExchange.Models.Orders;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test.TradingConnectors
{
    [TestClass]
    internal class BinanceTestConfigurationTest : BaseConfigurationTest
    {
        private IPrivateConnector _privateTestConnector;

        [TestInitialize]
        public void Init()
        {
            var dtoConverter = new CryptoExchangeConverter();
            ICredentials credentials = GetCredentials(Exchange.Binance);
            _privateTestConnector = new BinanceTestPrivateConnector(credentials, dtoConverter);
        }

        [TestMethod]
        public async Task Check_Test_PlaceOrder_Market_Method()
        {
            //Arrange
            var order = new MarketOrder("ETHBTC",
                OrderSide.Buy,
                0.1m,
                "test_clientOrderId",
                Exchange.Binance);

            //Act
            var orderResult = await _privateTestConnector.PlaceOrderAsync(order);

            //Assert
            Assert.AreEqual(orderResult, "0");
        }

        [TestMethod]
        public async Task Check_Test_PlaceOrder_Limit_Method()
        {
            //Arrange
            var order = new LimitOrder("ETHBTC",
                OrderSide.Buy,
                0.1m,
                0.040925m,
                "test_clientOrderId",
                Exchange.Binance);

            //Act
            var orderResult = await _privateTestConnector.PlaceOrderAsync(order);

            //Assert
            Assert.AreEqual(orderResult, "0");
        }
    }
}