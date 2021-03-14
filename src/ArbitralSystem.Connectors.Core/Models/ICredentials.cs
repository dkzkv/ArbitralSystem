using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models
{
    public interface IPrivateExchangeSettings : ICredentials, IExchange
    {
        long? AccountId { get; }        
    }
    
    public interface ICredentials 
    {
        string ApiKey { get; }
        string SecretKey { get; }
        string PassPhrase { get; }
    }
}