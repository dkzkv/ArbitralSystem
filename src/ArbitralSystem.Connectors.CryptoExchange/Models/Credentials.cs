using System;
using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Connectors.CryptoExchange.Models
{
    public class Credentials : ICredentials
    {
        public Credentials(string apiKey, string secretKey)
        {
            if(string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
                throw new ArgumentException("ApiKey or SecretKey is not valid.");
            
            ApiKey = apiKey;
            SecretKey = secretKey;
        }
        
        public Credentials(string apiKey, string secretKey, string passPhrase)
        {
            if(string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(secretKey))
                throw new ArgumentException("ApiKey or SecretKey is not valid.");
            
            if(string.IsNullOrEmpty(passPhrase))
                throw new ArgumentException("PassPhrase is not valid.");
            
            ApiKey = apiKey;
            SecretKey = secretKey;
            PassPhrase = passPhrase;
        }
        
        public string ApiKey { get; }
        public string SecretKey { get; }
        public string PassPhrase { get; }
    }
}