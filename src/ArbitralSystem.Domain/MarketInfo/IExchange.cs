using System;
using System.Collections;

namespace ArbitralSystem.Domain.MarketInfo
{
    public interface IExchange
    {
        public Exchange Exchange { get; }
    }

    public interface IExchangePairName
    {
        public string ExchangePairName { get; }
    }
    
    
}