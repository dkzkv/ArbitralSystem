using System.Linq;
using System.Threading.Tasks;
using Binance.Net.Interfaces;
using ArbitralSystem.Connectors.Core.Models;
using Binance.Net;


namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class BinanceWalletInfo
    {
        private readonly IBinanceClient _binanceClient;

        public BinanceWalletInfo(ICredentials credentials, IBinanceClient binanceClient = null)
        {
            if (binanceClient == null)
                _binanceClient = new BinanceClient();
            else
                _binanceClient = binanceClient;

            _binanceClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }

        public async Task Some()
        {

            var a1 = await _binanceClient.SubAccount.GetSubAccountBtcValuesAsync();
            var btcCoin = a1.Data.Where(o => o.Coin == "BTC").ToArray();
           // var a3 = await _binanceClient.SubAccount.GetSubAccountAssetsAsync();
            ;
        }
    }
}