using System;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Exceptions
{
    public class ExchangeCredentialsException : Exception
    {
        public ExchangeCredentialsException(Exchange exchange) : base($"Api exchange credentials is not valid for : {exchange}") { }
    }
}