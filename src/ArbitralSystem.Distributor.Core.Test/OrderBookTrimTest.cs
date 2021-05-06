using System;
using System.Linq;
using ArbitralSystem.Connectors.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArbitralSystem.Distributor.Core;
using ArbitralSystem.Distributor.Core.Common;
using ArbitralSystem.Domain.MarketInfo;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace ArbitralSystem.Distributor.Core.Test
{
    [TestClass]
    public class OrderBookTrimmerTest
    {
        private static Fixture _fixture;

        [ClassInitialize]
        public static void SetUp(TestContext ctx)
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
        }

        [DataTestMethod]
        [DataRow(3, 1, 5, 1, 3)]
        [DataRow(3, 5, 2, 3, 2)]
        [DataRow(3, 3, 3, 3, 3)]
        [DataRow(3, 0, 0, 0, 0)]
        [DataRow(3, 2, 2, 2, 2)]
        [DataRow(3, 0, 0, 0, 0)]
        public void OrderBookTrimTest(int trimLimit, int asksCount, int bidsCount, int expectedAsksCount, int expectedBidsCount)
        {
            var asks = _fixture.CreateMany<IOrderbookEntry>(asksCount);
            var bids = _fixture.CreateMany<IOrderbookEntry>(bidsCount);
            var orderbookEntry = _fixture.Create<IOrderbookEntry>();

            var orderBook = new DistributorOrderBook(Exchange.Undefined, "", DateTimeOffset.Now, bids,asks, orderbookEntry, orderbookEntry);

            var trimmedOrderBook = OrderbookTrimmer.Trim(orderBook, trimLimit);
            Assert.AreEqual(trimmedOrderBook.Asks.Count(), expectedAsksCount);
            Assert.AreEqual(trimmedOrderBook.Bids.Count(), expectedBidsCount);
        }
    }
}