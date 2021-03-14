using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test.SingleOrderBookDistributer
{
    [TestClass]
    public class BinanceOrderBookDistributerTest : BaseOrderBookDistributerTest
    {
        [TestMethod]
        public async Task BinanceDistributionDefaultOptions()
        {
            var options = new BinanceDistributerOptions {Frequency = 100};

            var orderbooks = new List<IDistributorOrderBook>();

            var binanceDistributer = new BinanceOrderbookDistributor(options, DtoConverter, Logger);
            binanceDistributer.OrderBookChanged += delegate(IDistributorOrderBook orderBook) { orderbooks.Add(orderBook); };
            var tokenSource = new CancellationTokenSource(TimeBeforeCancelDistribution);

            var pairInfo = new PairInfo
            {
                ExchangePairName = "ethbtc",
                BaseCurrency = "ETH",
                QuoteCurrency = "BTC"
            };

            var orderBookPairInfo = new OrderBookPairInfo(Exchange.Binance, pairInfo.ExchangePairName, pairInfo.UnificatedPairName);
            var distrTask = await binanceDistributer.StartDistributionAsync(orderBookPairInfo, tokenSource.Token);

            var tokenForTaskSource = new CancellationTokenSource(TimeBeforeCancelTask);

            distrTask.Wait(tokenForTaskSource.Token);

            Assert.AreEqual(distrTask.Status, TaskStatus.RanToCompletion);
            Assert.AreEqual(orderbooks.Any(), true);

            var orderbook = orderbooks.First();
            Assert.AreEqual(orderbook.Exchange, Exchange.Binance);

            Assert.IsTrue(orderbook.Asks.Count() <= 20);
            Assert.IsTrue(orderbook.Bids.Count() <= 20);

            if (orderbook.Asks.Any())
            {
                var ask = orderbook.Asks.First();
                Assert.AreEqual(orderbook.BestAsk.Quantity, ask.Quantity);
                Assert.AreEqual(orderbook.BestAsk.Price, ask.Price);
            }

            if (orderbook.Bids.Any())
            {
                var bid = orderbook.Bids.First();
                Assert.AreEqual(orderbook.BestBid.Quantity, bid.Quantity);
                Assert.AreEqual(orderbook.BestBid.Price, bid.Price);
            }
        }

        [TestMethod]
        public async Task BinanceDistribution_5_Options()
        {
            var options = new BinanceDistributerOptions
            {
                Frequency = 100
            };
            options.SetLimit(BinanceOrderBookLimit.limit_5);

            var orderbooks = new List<IDistributorOrderBook>();

            var binanceDistributer = new BinanceOrderbookDistributor(options, DtoConverter, Logger);
            binanceDistributer.OrderBookChanged += delegate(IDistributorOrderBook orderBook) { orderbooks.Add(orderBook); };
            var tokenSource = new CancellationTokenSource(TimeBeforeCancelDistribution);

            var pairInfo = new PairInfo
            {
                ExchangePairName = "ethbtc",
                BaseCurrency = "ETH",
                QuoteCurrency = "BTC"
            };
            var orderBookPairInfo = new OrderBookPairInfo(Exchange.Binance, pairInfo.ExchangePairName, pairInfo.UnificatedPairName);
            var distrTask = await binanceDistributer.StartDistributionAsync(orderBookPairInfo, tokenSource.Token);

            var tokenForTaskSource = new CancellationTokenSource(TimeBeforeCancelTask);

            distrTask.Wait(tokenForTaskSource.Token);

            Assert.AreEqual(distrTask.Status, TaskStatus.RanToCompletion);
            Assert.AreEqual(orderbooks.Any(), true);

            var orderbook = orderbooks.First();
            Assert.AreEqual(orderbook.Exchange, Exchange.Binance);

            Assert.IsTrue(orderbook.Asks.Count() <= 5);
            Assert.IsTrue(orderbook.Bids.Count() <= 5);

            if (orderbook.Asks.Any())
            {
                var ask = orderbook.Asks.First();
                Assert.AreEqual(orderbook.BestAsk.Quantity, ask.Quantity);
                Assert.AreEqual(orderbook.BestAsk.Price, ask.Price);
            }

            if (orderbook.Bids.Any())
            {
                var bid = orderbook.Bids.First();
                Assert.AreEqual(orderbook.BestBid.Quantity, bid.Quantity);
                Assert.AreEqual(orderbook.BestBid.Price, bid.Price);
            }
        }
    }
}