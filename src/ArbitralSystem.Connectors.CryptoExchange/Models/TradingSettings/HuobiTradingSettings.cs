using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.TradingSettings
{
    public class HuobiPrivateExchangeSettings : IPrivateExchangeSettings
    {
        public HuobiPrivateExchangeSettings(string apiKey, string secretKey, long accountId)
        {
            if(string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
                throw new ExchangeCredentialsException(Exchange);
            
            ApiKey = apiKey;
            SecretKey = secretKey;
            AccountId = accountId;
            PassPhrase = string.Empty;
        }
        public Exchange Exchange => Exchange.Binance;
        
        public long? AccountId { get; }
        public string ApiKey { get; }
        public string SecretKey { get; }
        public string PassPhrase { get; }
    }
}