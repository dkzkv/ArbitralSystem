using System.Linq;
using System.Threading;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Connectors.Integration.Test.TradingConnectors;
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
    public class NakedClients : BaseConfigurationTest
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
            var binanceCreds = Configuration.GetSection("BinanceCredentials").Get<TestPrivateSettings>();
            _binanceClient = new BinanceClient();
            _binanceClient.SetApiCredentials(binanceCreds.ApiKey, binanceCreds.SecretKey);
            
            _bittrexClient = new BittrexClient();
            _coinExClient = new CoinExClient();
            
            
            var huobiGreds = Configuration.GetSection("HuobiCredentials").Get<TestPrivateSettings>();
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
            /*GetSymbolsResponse symbolsData = _huobiCommonClient.GetSymbolsAsync().Result;
            var symbols = symbolsData.data.Where(o => o.apiTrading == "enabled").Select(o => o.symbol).Take(10);
            var allSymbols = string.Join(',', symbols);
            var request = new GetRequest()
                .AddParam("symbols", allSymbols);
            var a = _huobiOrderClient.GetTransactFeeRateAsync(request).Result;

            var request1 = new GetRequest()
                .AddParam("currency", "btc");

            GetWithdrawQuotaResponse result = _huobiWalletClient.GetWithdrawQuotaAsync(request1).Result;
            var currencysResponse = _huobiCommonClient.GetCurrencyAsync("btc", false).Result*/
            ;
            
            //var symbols = _huobiClient.GetSymbols();
            //withdrow свой клиент

            var exInfo = _binanceClient.Spot.System.GetExchangeInfo();
            var accInfo = _binanceClient.General.GetAccountInfo();
            var coins = _binanceClient.General.GetUserCoins();
            ;
            //var accStatus = _binanceClient.General.GetAccountStatus();
            // var accTrStatus = _binanceClient.General.GetTradingStatus();
        }
    }
}