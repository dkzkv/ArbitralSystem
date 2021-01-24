using ArbitralSystem.Connectors.CoinEx.Models;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Domain.MarketInfo;
using Binance.Net.Objects;
using Bittrex.Net.Objects;
using Huobi.Net.Objects;
using Kraken.Net.Objects;
using Kucoin.Net.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using Binance.Net.Objects.Spot.MarketData;

namespace ArbitralSystem.Connectors.Test
{
    [TestClass]
    public class PairInfoConverterTest
    {
        private readonly IDtoConverter _dtoConverter;

        public PairInfoConverterTest()
        {
            _dtoConverter = new CryptoExchangeConverter();
        }

        [DataTestMethod]
        [DataRow("ETHBTC", "ETH", "BTC")]
        [DataRow("ETHBTC", "eth", "btc")]
        [DataRow("", "eth", "")]
        [DataRow(null, "eth", "")]
        [DataRow(null, null, "")]
        public void BinanceSymbolConverter(string pair, string baseCurrency, string quoteCurrency)
        {
            var rawSymbol = new BinanceSymbol
            {
                Name = pair,
                BaseAsset = baseCurrency,
                QuoteAsset = quoteCurrency
            };

            var convertedDto = _dtoConverter.Convert<BinanceSymbol, PairInfo>(rawSymbol);

            Assert.AreEqual(convertedDto.Exchange, Exchange.Binance);
            Assert.AreEqual(convertedDto.BaseCurrency, baseCurrency);
            Assert.AreEqual(convertedDto.QuoteCurrency, quoteCurrency);
            Assert.AreEqual(convertedDto.ExchangePairName, pair);
            Assert.AreEqual(convertedDto.UnificatedPairName, GetUnificatedPairName(baseCurrency, quoteCurrency));
        }

        [DataTestMethod]
        [DataRow("ETHBTC", "ETH", "BTC")]
        [DataRow("ETHBTC", "eth", "btc")]
        [DataRow("", "eth", "")]
        [DataRow(null, "eth", "")]
        [DataRow(null, null, "")]
        public void BittrexSymbolConverter(string pair, string baseCurrency, string quoteCurrency)
        {
            var rawSymbol = new BittrexSymbol
            {
                Symbol = pair,
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency
            };

            var convertedDto = _dtoConverter.Convert<BittrexSymbol, PairInfo>(rawSymbol);

            Assert.AreEqual(convertedDto.Exchange, Exchange.Bittrex);
            Assert.AreEqual(convertedDto.BaseCurrency, baseCurrency);
            Assert.AreEqual(convertedDto.QuoteCurrency, quoteCurrency);
            Assert.AreEqual(convertedDto.ExchangePairName, pair);
            Assert.AreEqual(convertedDto.UnificatedPairName, GetUnificatedPairName(baseCurrency, quoteCurrency));
        }

        private class TestMarketInfo : IMarketInfo
        {
            public decimal FreeRate { get; set; }
            public string PricingName { get; set; }
            public string TradingName { get; set; }
            public string Name { get; set; }
            public decimal MinAmount { get; set; }
            public int TradingPrecision { get; set; }
            public decimal MakerFee { get; set; }
            public int PricingPrecision { get; set; }
        }
        
        [DataTestMethod]
        [DataRow("ETHBTC", "ETH", "BTC")]
        [DataRow("ETHBTC", "eth", "btc")]
        [DataRow("", "eth", "")]
        [DataRow(null, "eth", "")]
        [DataRow(null, null, "")]
        public void CoinExSymbolConverter(string pair, string baseCurrency, string quoteCurrency)
        {
            var rawSymbol = new TestMarketInfo
            {
                Name = pair,
                TradingName = baseCurrency,
                PricingName = quoteCurrency
            };

            var convertedDto = _dtoConverter.Convert<IMarketInfo, PairInfo>(rawSymbol);

            Assert.AreEqual(convertedDto.Exchange, Exchange.CoinEx);
            Assert.AreEqual(convertedDto.BaseCurrency, baseCurrency);
            Assert.AreEqual(convertedDto.QuoteCurrency, quoteCurrency);
            Assert.AreEqual(convertedDto.ExchangePairName, pair);
            Assert.AreEqual(convertedDto.UnificatedPairName, GetUnificatedPairName(baseCurrency, quoteCurrency));
        }

        [DataTestMethod]
        [DataRow("ETHBTC", "ETH", "BTC")]
        [DataRow("ETHBTC", "eth", "btc")]
        [DataRow("", "eth", "")]
        [DataRow(null, "eth", "")]
        [DataRow(null, null, "")]
        public void HuobiSymbolConverter(string pair, string baseCurrency, string quoteCurrency)
        {
            var rawSymbol = new HuobiSymbol
            {
                Symbol = pair,
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency
            };

            var convertedDto = _dtoConverter.Convert<HuobiSymbol, PairInfo>(rawSymbol);

            Assert.AreEqual(convertedDto.Exchange, Exchange.Huobi);
            Assert.AreEqual(convertedDto.BaseCurrency, baseCurrency);
            Assert.AreEqual(convertedDto.QuoteCurrency, quoteCurrency);
            Assert.AreEqual(convertedDto.ExchangePairName, pair);
            Assert.AreEqual(convertedDto.UnificatedPairName, GetUnificatedPairName(baseCurrency, quoteCurrency));
        }

        [DataTestMethod]
        [DataRow("ETHBTC", "ETH", "BTC")]
        [DataRow("ETHBTC", "eth", "btc")]
        [DataRow("", "eth", "")]
        [DataRow(null, "eth", "")]
        [DataRow(null, null, "")]
        public void KrakenSymbolConverter(string pair, string baseCurrency, string quoteCurrency)
        {
            var rawSymbol = new KrakenSymbol
            {
                WebsocketName = pair,
                BaseAsset = baseCurrency,
                QuoteAsset = quoteCurrency
            };

            var convertedDto = _dtoConverter.Convert<KrakenSymbol, PairInfo>(rawSymbol);

            Assert.AreEqual(convertedDto.Exchange, Exchange.Kraken);
            Assert.AreEqual(convertedDto.BaseCurrency, baseCurrency);
            Assert.AreEqual(convertedDto.QuoteCurrency, quoteCurrency);
            Assert.AreEqual(convertedDto.ExchangePairName, pair);
            Assert.AreEqual(convertedDto.UnificatedPairName, GetUnificatedPairName(baseCurrency, quoteCurrency));
        }

        [DataTestMethod]
        [DataRow("ETHBTC", "ETH", "BTC")]
        [DataRow("ETHBTC", "eth", "btc")]
        [DataRow("", "eth", "")]
        [DataRow(null, "eth", "")]
        [DataRow(null, null, "")]
        public void KucoinSymbolConverter(string pair, string baseCurrency, string quoteCurrency)
        {
            var rawSymbol = new KucoinSymbol
            {
                Symbol = pair,
                BaseCurrency = baseCurrency,
                QuoteCurrency = quoteCurrency
            };

            var convertedDto = _dtoConverter.Convert<KucoinSymbol, PairInfo>(rawSymbol);

            Assert.AreEqual(convertedDto.Exchange, Exchange.Kucoin);
            Assert.AreEqual(convertedDto.BaseCurrency, baseCurrency);
            Assert.AreEqual(convertedDto.QuoteCurrency, quoteCurrency);
            Assert.AreEqual(convertedDto.ExchangePairName, pair);
            Assert.AreEqual(convertedDto.UnificatedPairName, GetUnificatedPairName(baseCurrency, quoteCurrency));
        }


        private string GetUnificatedPairName(string baseCurrency, string quoteCurrency)
        {
            baseCurrency = baseCurrency?.ToUpper();
            quoteCurrency = quoteCurrency?.ToUpper();
            if (baseCurrency != null && quoteCurrency != null)
                return $"{baseCurrency}/{quoteCurrency}";
            return string.Empty;
        }
    }
}