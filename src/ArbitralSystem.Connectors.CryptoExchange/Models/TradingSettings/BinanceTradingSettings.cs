using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.TradingSettings
{
    
    public class BinancePrivateExchangeSettings : IPrivateExchangeSettings
    {
        public BinancePrivateExchangeSettings(string apiKey, string secretKey)
        {
            if(string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
                throw new ExchangeCredentialsException(Exchange);
            
            ApiKey = apiKey;
            SecretKey = secretKey;
            PassPhrase = string.Empty;
            AccountId = null;
        }
        public Exchange Exchange => Exchange.Binance;
        public string ApiKey { get; }
        public string SecretKey { get; }
        public string PassPhrase { get; }
        public long? AccountId { get;  }
    }
}