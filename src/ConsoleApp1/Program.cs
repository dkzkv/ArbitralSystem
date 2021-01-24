using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Bitfinex;
using ArbitralSystem.Connectors.Core.Distributers;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.CryptoExchange;
using ArbitralSystem.Connectors.CryptoExchange.Common;
using ArbitralSystem.Connectors.CryptoExchange.Converter;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors;
using ArbitralSystem.Domain.MarketInfo;
using ArbitralSystem.Test;

namespace ConsoleApp1
{
    class Program
    {
        private static string ApiKey = "";
        private static string Secret = "";
        
        static async Task Main(string[] args)
        {
            var creds = new Credentials(ApiKey,Secret);

            var accountInfo = new BinanceWalletInfo(creds);
            await accountInfo.Some();
            
            ;
            return;
            /* var factory = CreateDistributerFactory();
             var distributor = factory.GetInstance(Exchange.Bitfinex);
             
             distributor.OrderBookChanged += DistributorOnOrderBookChanged;
             distributor.DistributerStateChanged += DistributorOnDistributerStateChanged;
             
             var cancellationTokenSource = new CancellationTokenSource();
 
             var distrTask = await distributor.StartDistributionAsync(new OrderBookPairInfo(Exchange.Bitfinex, "tLTCUSD", "XBT/USD"), cancellationTokenSource.Token);
 
             await distrTask;*/

        }

        private static void DistributorOnDistributerStateChanged(IDistributerState state)
        {
           Console.WriteLine($"state: {state.CurrentStatus}");
        }

        private static void DistributorOnOrderBookChanged(IOrderBook orderbook)
        {
            //Console.WriteLine($"Asks: {orderbook.Asks.Count()}, Bids: {orderbook.Bids.Count()}");
            Console.WriteLine($"Ask: {orderbook.BestAsk.Price}, Bid: {orderbook.BestBid.Price}");
        }
        
        private static IOrderBookDistributerFactory CreateDistributerFactory()
        {
            var options = new DistributerOptions {Frequency = 100};
            return  new CryptoExOrderBookDistributerFactory(options,
                new CryptoExchangeConverter(),
                new EmptyLogger());
        }
    }
}