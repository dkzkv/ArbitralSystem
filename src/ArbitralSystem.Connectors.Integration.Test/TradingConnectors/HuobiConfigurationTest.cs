using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test.TradingConnectors
{
    [TestClass]
    public class HuobiConfigurationTest : BaseConfigurationTest
    {
        private IPrivateConnector _privateConnector;

        [TestInitialize]
        public void Init()
        {
            var dtoConverter = new CryptoExchangeConverter();
            var credentials = Configuration.GetSection("HuobiCredentials").Get<TestPrivateSettings>();
            _privateConnector = new HuobiPrivateConnector(credentials.AccountID, credentials, dtoConverter);
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
    }
}