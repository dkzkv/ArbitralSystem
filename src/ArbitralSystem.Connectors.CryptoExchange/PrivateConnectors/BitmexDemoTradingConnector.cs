using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Converters;
using Bitmex.Net.Client.Interfaces;
using Bitmex.Net.Client.Objects;
using Bitmex.Net.Client.Objects.Requests;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class BitmexDemoTradingConnector
    {
        private readonly IBitmexClient _bitmexClient;
        private readonly IDtoConverter _converter;
        
        public BitmexDemoTradingConnector(IBitmexClient bitmexClient, IDtoConverter converter)
        {
            _bitmexClient = bitmexClient;
            _converter = converter;
        }

        public async Task PlaceOrder(string symbol)
        {
            await _bitmexClient.PlaceOrderAsync(new PlaceOrderRequest(symbol)
            {
                Side = BitmexOrderSide.Buy,
                BitmexOrderType = BitmexOrderType.Market,
                
            });
        }

        public async Task GetOpenOrders()
        {
           // await _bitmexClient.CancelOrderAsync()
        }
        
        public async Task Get()
        {
            //await _bitmexClient()
        }
        
        
    }
}