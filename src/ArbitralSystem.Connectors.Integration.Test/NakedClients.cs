using System.Linq;
using System.Threading;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Connectors.Integration.Test.TradingConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Binance.Net;
using Binance.Net.Interfaces;
using Bitfinex.Net;
using Bitfinex.Net.Interfaces;
using Bitfinex.Net.Objects;
using Bitmex.Net.Client;
using Bitmex.Net.Client.Interfaces;
using Bitmex.Net.Client.Objects.Requests;
using Bittrex.Net;
using Bittrex.Net.Interfaces;
using CoinEx.Net;
using CoinEx.Net.Interfaces;
using HitBTC.Net;
using HitBTC.Net.Interfaces;
using Huobi.Net;
using Huobi.Net.Interfaces;
using Huobi.SDK.Core;
using Huobi.SDK.Core.Client;
using Huobi.SDK.Model.Response.Common;
using Huobi.SDK.Model.Response.Wallet;
using Kraken.Net;
using Kraken.Net.Interfaces;
using Kucoin.Net;
using Kucoin.Net.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test
{
    [TestClass]
    internal class NakedClients : BaseConfigurationTest
    {
        private IBinanceClient _binanceClient;
        private IBittrexClient _bittrexClient;
        private ICoinExClient _coinExClient;
        private IHuobiClient _huobiClient;
        private WalletClient _huobiWalletClient;
        private OrderClient _huobiOrderClient;
        private CommonClient _huobiCommonClient;
        private IKrakenClient _krakenClient;
        private IKucoinClient _kucoinClient;
        private IBitmexClient _bitmexClient;
        private IBitfinexClient _bitfinexClient;
        


        [TestInitialize]
        public void Init()
        {
            
            var binanceCreds = GetCredentials(Exchange.Binance);
            _binanceClient = new BinanceClient();
            _binanceClient.SetApiCredentials(binanceCreds.ApiKey, binanceCreds.SecretKey);
            
            _bittrexClient = new BittrexClient();
            _coinExClient = new CoinExClient();
            
            
            var huobiGreds = GetCredentials(Exchange.Huobi);
            _huobiClient = new HuobiClient();
            _huobiClient.SetApiCredentials(huobiGreds.ApiKey, huobiGreds.SecretKey);
            _huobiCommonClient = new CommonClient();
            _huobiOrderClient = new OrderClient(huobiGreds.ApiKey, huobiGreds.SecretKey);
            _huobiWalletClient = new WalletClient(huobiGreds.ApiKey, huobiGreds.SecretKey);
            _krakenClient = new KrakenClient();
            _kucoinClient = new KucoinClient();
            _bitmexClient = new BitmexClient();
            _bitfinexClient = new BitfinexClient();

        }

        [TestMethod]
        public void TestClients()
        {
        }
    }
}