using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Distributers;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Connectors.Integration.Test.SingleOrderBookDistributer
{
    [TestClass]
    public class KrakenOrderBookDistributerTest : BaseOrderBookDistributerTest
    {
        [TestMethod]
        public async Task KrakenDistributionDefaultOptions()
        {
            var options = new DistributerOptions {Frequency = 100};

            var orderbooks = new List<IOrderBook>();

            var distributer = new KrakenOrderbookDistributor(options, DtoConverter, Logger);
            distributer.OrderBookChanged += delegate(IOrderBook orderBook) { orderbooks.Add(orderBook); };
            var tokenSource = new CancellationTokenSource(TimeBeforeCancelDistribution);

            var pairInfo = new PairInfo
            {
                ExchangePairName = "BTC/USD",
                BaseCurrency = "ETH",
                QuoteCurrency = "BTC"
            };
            var orderBookPairInfo = new OrderBookPairInfo(Exchange.Kraken, pairInfo.ExchangePairName, pairInfo.UnificatedPairName);
            var distrTask = await distributer.StartDistributionAsync(orderBookPairInfo, tokenSource.Token);

            var tokenForTaskSource = new CancellationTokenSource(TimeBeforeCancelTask);

            distrTask.Wait(tokenForTaskSource.Token);

            Assert.AreEqual(distrTask.Status, TaskStatus.RanToCompletion);
            Assert.AreEqual(orderbooks.Any(), true);

            var orderbook = orderbooks.First();
            Assert.AreEqual(orderbook.Exchange, Exchange.Kraken);

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
        [Ignore]
        public async Task KrakenFullDistibutions()
        {
            var options = new DistributerOptions {Frequency = 100};

            IPublicConnector publicConnector = new KrakenConnector(DtoConverter);
            var pairs = await publicConnector.GetPairsInfo();

            Assert.IsTrue(pairs.Any());

            var counter = 0;
            foreach (var pairInfo in pairs)
            {
                var orderbooks = new List<IOrderBook>();
                var distributer = new KrakenOrderbookDistributor(options, DtoConverter, Logger);
                distributer.OrderBookChanged += delegate(IOrderBook orderBook) { orderbooks.Add(orderBook); };
                var tokenSource = new CancellationTokenSource(TimeBeforeCancelDistribution);
                var orderBookPairInfo = new OrderBookPairInfo(Exchange.Kraken, pairInfo.ExchangePairName, pairInfo.UnificatedPairName);
                var distrTask = await distributer.StartDistributionAsync(orderBookPairInfo, tokenSource.Token);

                var tokenForTaskSource = new CancellationTokenSource(TimeBeforeCancelTask);

                distrTask.Wait(tokenForTaskSource.Token);

                Assert.AreEqual(distrTask.Status, TaskStatus.RanToCompletion);
                Assert.AreEqual(orderbooks.Any(), true);

                var orderbook = orderbooks.First();
                Assert.AreEqual(orderbook.Exchange, Exchange.Kraken);

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

                counter++;
            }

            Assert.AreEqual(pairs.Count(), counter);
        }
    }
}