using System;
using System.Linq;
using ArbitralSystem.Distributor.Core.Common;
using ArbitralSystem.Domain.MarketInfo;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArbitralSystem.Distributor.Core.Test
{
    [TestClass]
    public class DistributorOrderBookEqualityTest
    {
        private static Fixture _fixture;

        [ClassInitialize]
        public static void SetUp(TestContext ctx)
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoMoqCustomization());
        }
        
        [TestMethod]
        public void DistributorOrderBook_Equality_Test()
        {
            //Arrange
            var asks = _fixture.CreateMany<TestOrderBookEntry>(3).OrderBy(o=>o.Price).ToArray();
            var bids = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid = bids.First();
            var bestAsk = asks.First();
            
            var orderBook1 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);
            var orderBook2 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);
            
            //Act
            //Assert
            Assert.IsTrue(orderBook1.Equals(orderBook2));
            Assert.IsTrue(orderBook1.GetHashCode().Equals(orderBook2.GetHashCode()));
        }
        
        [TestMethod]
        public void DistributorOrderBook_Equality_AnotherEntryLink_Test()
        {
            //Arrange
            var asks = _fixture.CreateMany<TestOrderBookEntry>(3).OrderBy(o=>o.Price).ToArray();
            var bids = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid = bids.First();
            var bestAsk = asks.First();
            
            var orderBook1 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);
            
            var newBestAsk = new TestOrderBookEntry
            {
                Price = bestAsk.Price,
                Quantity = bestAsk.Quantity
            };
            var orderBook2 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, newBestAsk);

            //Act
            //Assert
            Assert.IsTrue(orderBook1.Equals(orderBook2));
            Assert.IsTrue(orderBook1.GetHashCode().Equals(orderBook2.GetHashCode()));
        }
        
        [TestMethod]
        public void DistributorOrderBook_NonEquality_BestAsk_Test()
        {
            //Arrange
            var asks = _fixture.CreateMany<TestOrderBookEntry>(3).OrderBy(o=>o.Price).ToArray();
            var bids = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid = bids.First();
            var bestAsk = asks.First();
            var orderBook1 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);

            var newBestAsk = new TestOrderBookEntry
            {
                Price = bestAsk.Price,
                Quantity = bestAsk.Quantity + 1
            };
            var orderBook2 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, newBestAsk);
            
            //Act
            //Assert
            Assert.IsFalse(orderBook1.Equals(orderBook2));
            Assert.IsTrue(orderBook1.GetHashCode().Equals(orderBook2.GetHashCode()));
        }
        
        [TestMethod]
        public void DistributorOrderBook_NonEquality_WrongOrder_Test()
        {
            //Arrange
            var asks = _fixture.CreateMany<TestOrderBookEntry>(3).OrderBy(o=>o.Price).ToArray();
            var bids = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid = bids.First();
            var bestAsk = asks.First();

            var wrongOrderedBids = bids.OrderBy(o => o.Price).ToArray();
            var orderBook1 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);
            var orderBook2 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, wrongOrderedBids,asks, bestBid, bestAsk);
            
            //Act
            //Assert
            Assert.IsFalse(orderBook1.Equals(orderBook2));
            Assert.IsTrue(orderBook1.GetHashCode().Equals(orderBook2.GetHashCode()));
        }
        
        [TestMethod]
        public void DistributorOrderBook_NonEquality_WrongHash_Test()
        {
            //Arrange
            var asks = _fixture.CreateMany<TestOrderBookEntry>(3).OrderBy(o=>o.Price).ToArray();
            var bids = _fixture.CreateMany<TestOrderBookEntry>(3).OrderByDescending(o=>o.Price).ToArray();
            var bestBid = bids.First();
            var bestAsk = asks.First();
            
            var orderBook1 = new DistributorOrderBook(Exchange.Binance, "test1", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);
            var orderBook2 = new DistributorOrderBook(Exchange.Binance, "test2", DateTimeOffset.Now, bids,asks, bestBid, bestAsk);
            
            //Act
            //Assert
            Assert.IsFalse(orderBook1.Equals(orderBook2));
            Assert.IsFalse(orderBook1.GetHashCode().Equals(orderBook2.GetHashCode()));
        }
        
    }
}