using System;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Connectors.CryptoExchange.Models.Orders;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.Text;

namespace ArbitralSystem.Connectors.Integration.Test.TradingConnectors
{
    [TestClass]
    internal class HuobiConfigurationTest : BaseConfigurationTest
    {
        private IPrivateConnector _privateConnector;

        [TestInitialize]
        public void Init()
        {
            var dtoConverter = new CryptoExchangeConverter();
            var credentials = GetCredentials(Exchange.Huobi);
            _privateConnector = new HuobiPrivateConnector(credentials.AccountId ?? throw new ArgumentException("Account id not defined for huobi settings."),
                credentials, dtoConverter);
        }
        
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
            const string symbol = "ethbtc";

            //Act
            var orders = await _privateConnector.GetOpenOrdersAsync(symbol);

            //Assert
            Assert.IsNotNull(orders);
        }
        
        // [Ignore]
        [TestMethod]
        public async Task Check_PlaceOrder_Market_Method()
        {
            //Arrange
            const string symbol = "iotabtc";
            var id = DateTime.Now.ToUnixTime().ToString();
            string clientOrderId = $"{id}";

            var order = new MarketOrder(symbol,
                OrderSide.Sell,
                5m,
                null,
                Exchange.Huobi);
            
            
            var limitOrder = new LimitOrder(symbol,
                OrderSide.Buy,
                3m,
                decimal.Round(0.00004098m * 1.50m,8, MidpointRounding.AwayFromZero) ,
                null,
                Exchange.Huobi);
            
            // var limitOrder = new LimitOrder(symbol,
            //     OrderSide.Sell,
            //     3m,
            //     decimal.Round(0.00004098m * 0.92m,8, MidpointRounding.AwayFromZero) ,
            //     null,
            //     Exchange.Huobi);

            var bal = await _privateConnector.GetBalanceAsync();
            //Act
            var orderResult = await _privateConnector.PlaceOrderAsync(limitOrder);
            
            //Assert
            Assert.IsFalse(string.IsNullOrEmpty(orderResult));
        }
    }
}