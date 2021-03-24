using System;
using System.Linq;
using ArbitralSystem.Distributor.Core.Common;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Test;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Distributor.Core.Test
{
    [TestClass]
    public class LockedOrderBookDeDuplicatorTest
    {
        private static Fixture _fixture;

        [ClassInitialize]
        public static void SetUp(TestContext ctx)
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
        }
        
        [TestMethod]
        public void OrderBookDeDuplicator_Duplicate()
        {
            //Arrange
            var asks1 = _fixture.CreateMany<TestOrderBookEntry>(3).OrderBy(o=>o.Price).ToArray();
            var bids1 = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid1 = bids1.First();
            var bestAsk1 = asks1.First();
            var orderBook1 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids1,asks1, bestBid1, bestAsk1);
            
            var asks2 = _fixture.CreateMany<TestOrderBookEntry>(4).OrderBy(o=>o.Price).ToArray();
            var bids2 = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid2 = bids1.First();
            var bestAsk2 = asks1.First();
            var orderBook2 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids2,asks2, bestBid2, bestAsk2);

            var deDuplicator = new LockedOrderBookDeDuplicator();
            
            var firstOrderBookResult = deDuplicator.IsDuplicate(orderBook1);
            var secondOrderBookResult = deDuplicator.IsDuplicate(orderBook2);
            //Act
            //Assert
            Assert.IsFalse(firstOrderBookResult);
            Assert.IsFalse(secondOrderBookResult);
        }
    }
}