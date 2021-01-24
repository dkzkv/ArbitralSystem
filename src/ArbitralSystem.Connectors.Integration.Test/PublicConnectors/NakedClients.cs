using System.Linq;
using System.Threading;
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
using Kraken.Net;
using Kraken.Net.Interfaces;
using Kucoin.Net;
using Kucoin.Net.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.PublicConnectors.Test
{
    [TestClass]
    public class NakedClients
    {
        private IBinanceClient _binanceClient;
        private IBittrexClient _bittrexClient;
        private ICoinExClient _coinExClient;
        private IHuobiClient _huobiClient;
        private IKrakenClient _krakenClient;
        private IKucoinClient _kucoinClient;
        private IBitmexClient _bitmexClient;
        private IBitfinexClient _bitfinexClient;


        [TestInitialize]
        public void Init()
        {
            _binanceClient = new BinanceClient();
            _bittrexClient = new BittrexClient();
            _coinExClient = new CoinExClient();
            _huobiClient = new HuobiClient();
            _krakenClient = new KrakenClient();
            _kucoinClient = new KucoinClient();
            _bitmexClient = new BitmexClient();
            _bitfinexClient = new BitfinexClient();

        }

        [TestMethod]
        public void TestClients()
        {
            var symbols = _bitfinexClient.GetSymbols();
            var symbolsDetails = _bitfinexClient.GetSymbolDetails();
        }
    }
}