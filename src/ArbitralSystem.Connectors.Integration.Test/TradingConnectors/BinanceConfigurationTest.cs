using System;
using System.Linq;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary;
using ArbitralSystem.Connectors.CryptoExchange.Models.Orders;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Text;

namespace ArbitralSystem.Connectors.Integration.Test.TradingConnectors
{
    [TestClass]
    public class BinanceConfigurationTest : BaseConfigurationTest
    {
        private IPrivateConnector _privateConnector;

        [TestInitialize]
        public void Init()
        {
            var dtoConverter = new CryptoExchangeConverter();
            ICredentials credentials = Configuration.GetSection("BinanceCredentials").Get<TestPrivateSettings>();
            _privateConnector = new BinancePrivateConnector(credentials, dtoConverter);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Check_PlaceOrder_Method_Not_Valid_Argument()
        {
            //Arrange
            var order = new Order()
            {
                Exchange = Exchange.Undefined
            };

            //Act
            var orders = await _privateConnector.PlaceOrderAsync(order);
        }

        #region Ingored, related to balance

       // [Ignore]
        [TestMethod]
        public async Task Check_PlaceOrder_Market_Method()
        {
            //Arrange
            const string symbol = "IOTABTC";
            var id = DateTime.Now.ToUnixTime().ToString();
            string clientOrderId = $"SPIIOTABTC{id}";

            var order = new MarketOrder(symbol,
                OrderSide.Sell,
                5m,
                clientOrderId,
                Exchange.Binance);

            //Act
            var orderResult = await _privateConnector.PlaceOrderAsync(order);
            
            //Assert
            Assert.IsFalse(string.IsNullOrEmpty(orderResult));
        }

        [Ignore]
        [TestMethod]
        public async Task Check_GetOrder_NotActive_Method()
        {
            //Arrange
            const string symbol = "VETBTC";
            const string orderId = "116116700";
            const string clientOrderId = "test_clientOrderId";
            var request = new OrderRequest(symbol, orderId, clientOrderId);

            //Act
            var myOrder = await _privateConnector.GetOrderAsync(request);

            //Assert
            Assert.AreEqual(myOrder.ExchangePairName, symbol);
            Assert.AreEqual(myOrder.Id, orderId);
            Assert.IsFalse(myOrder.IsActive);
        }
        
        [Ignore]
        [TestMethod]
        public async Task Check_GetLimitOrder_And_Cancel()
        {
            //Arrange
            const string symbol = "IOTABTC";
            const string clientOrderId = "Check_GetLimitOrder_And_Cancel";

            var order = new LimitOrder(symbol,
                OrderSide.Buy,
                10m,
                0.00001m,
                clientOrderId,
                Exchange.Binance);

            //Act
            var orderResult = await _privateConnector.PlaceOrderAsync(order);
            Assert.IsFalse(string.IsNullOrEmpty(orderResult));
            
            var openOrder = await _privateConnector.GetOpenOrdersAsync(symbol);
            Assert.IsTrue(openOrder.Any());

            var cancelOrder = new CancelOrder(Exchange.Binance, symbol, orderResult, clientOrderId);
            await _privateConnector.CancelOrderAsync(cancelOrder);
        }

        #endregion
        
        [TestMethod]
        public async Task Check_Balances_Method()
        {
            //Act
            var balance = await _privateConnector.GetBalanceAsync();

            //Assert
            Assert.IsNotNull(balance);
        }

        [TestMethod]
        public async Task Check_OpenOrders_Method()
        {
            //Arrange
            const string symbol = "ETHBTC";

            //Act
            var orders = await _privateConnector.GetOpenOrdersAsync(symbol);

            //Assert
            Assert.IsNotNull(orders);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Check_OpenOrders_Method_Not_Valid_Argument()
        {
            //Arrange
            const string symbol = null;

            //Act
            var orders = await _privateConnector.GetOpenOrdersAsync(symbol);
        }
    }
}