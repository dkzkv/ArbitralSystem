using System;
using ArbitralSystem.Connectors.Core.Common;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Domain.MarketInfo;
using CryptoExchange.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    public abstract class BaseConnector
    {
        public virtual void ValidateResponse<T>(IResponse<T> result)
        {
            if (!result.IsSuccess)
                throw new RestClientException(result.Exception?.Message);
        }
        
        public virtual void ValidateResponse<T>(WebCallResult<T> result)
        {
            if (!result.Success)
                throw new RestClientException(result.Error?.Message);
        }
        
        public virtual void ValidateResponse<T>(CallResult<T> result)
        {
            if (!result.Success)
                throw new RestClientException(result.Error?.Message);
        }
    }
    
    public abstract class ExchangeBaseConnector : BaseConnector , IExchange
    {
        public abstract Exchange Exchange { get; }

        public void ValidateExchangeArgument(IExchange exchangeArg)
        {
            if(exchangeArg.Exchange != Exchange)
                throw new ArgumentException($"Exchange argument: {exchangeArg.Exchange} not belongs to target exchange connector: {Exchange}");
        }
        
        public override void ValidateResponse<T>(IResponse<T> result)
        {
            if (!result.IsSuccess)
                throw new RestClientException($"Error in: {Exchange}",result.Exception);
        }
        
        public override void ValidateResponse<T>(WebCallResult<T> result)
        {
            if (!result.Success)
                throw new RestClientException($"Error in: {Exchange}, Message: {result.Error.Message}");
        }
        
        public override void ValidateResponse<T>(CallResult<T> result)
        {
            if (!result.Success)
                throw new RestClientException($"Error in: {Exchange}, Message: {result.Error.Message}");
        }
    }
    
}