using System.Linq;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange;
using ArbitralSystem.Connectors.Integration.Test.TradingConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test.AccountConnectors
{
    [TestClass]
    public class ExchangeAccountConnectorTest : BaseConfigurationTest
    {
        private  IAccountConnectorFactory _accountConnectorFactory;
        
        [TestInitialize]
        public void Init()
        {
            var credentials = Configuration.GetSection("TradingSettings").Get<TestPrivateSettings[]>().ToArray<IPrivateExchangeSettings>();
            _accountConnectorFactory= new CryptoExAccountConnectorFactory(credentials);
        }

        [TestMethod]
        [DataRow(Exchange.Binance,"ETHBTC")]
        [DataRow(Exchange.Huobi,"ethbtc")]
        public async Task Check_FeeRate_Method(Exchange exchange, string exchangePairName)
        {
            //Arrange
            var accountConnector = _accountConnectorFactory.GetInstance(exchange);
            
            //Act
            var feeRate = await accountConnector.GetFeeRateAsync(exchangePairName);

            //Assert
            Assert.IsNotNull(feeRate);
            Assert.AreEqual(feeRate.Exchange, exchange);
            Assert.AreEqual(feeRate.ExchangePairName, exchangePairName);
        }
        
        [TestMethod]
        [DataRow(Exchange.Binance,"BTC","BTC")]
        [DataRow(Exchange.Huobi,"btc", "BTC")]
        public async Task Check_WithdrawCommission_Method(Exchange exchange, string currency, string chainName)
        {
            var accountConnector = _accountConnectorFactory.GetInstance(exchange);
            
            //Act
            var withdrawCommission = await accountConnector.GetWithdrawCommissionAsync(currency);
            var chainCommission = withdrawCommission.FirstOrDefault(o => o.ChainName == chainName);
            
            //Assert
            Assert.IsNotNull(chainCommission);
            Assert.AreNotEqual(chainCommission.Commission, 0);
            Assert.AreNotEqual(chainCommission.MinWithdrawAmount, 0);
        }
    }
}