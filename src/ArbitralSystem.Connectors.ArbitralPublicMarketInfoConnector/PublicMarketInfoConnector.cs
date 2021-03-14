using System.Collections.Generic;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models;
using ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector.Models.Auxiliary;
using ArbitralSystem.Connectors.Core;
using ArbitralSystem.Connectors.Core.Common;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;
using RestSharp;

namespace ArbitralSystem.Connectors.ArbitralPublicMarketInfoConnector
{
    public class PublicMarketInfoConnector : BaseRestClient ,IPublicMarketInfoConnector
    {
        private const string Version = "v1";
        
        public PublicMarketInfoConnector(string baseUrl) : base(baseUrl)
        {
        }
        
        public PublicMarketInfoConnector(string baseUrl, ConnectionOptions connectionOptions) : base(baseUrl, connectionOptions)
        {
        }

        [ItemNotNull]
        public async Task<IResponse<IEnumerable<IArbitralPairInfo>>> GetPairs(Exchange exchange)
        {
            var restRequest = new RestRequest($"api/{Version}/pair-info/{exchange.ToString()}");
            restRequest.Method = Method.GET;
            
            var response = await ExecuteRequestWithTimeOut(restRequest);
            return  DeserializeResponse<IEnumerable<ArbitralPairInfo>, IEnumerable<IArbitralPairInfo>>(response);
        }

        public async Task<IResponse<IPage<IArbitralPairInfo>>> GetPairs(ArbitralPairInfoFilterFilter filter)
        {
            var restRequest = new RestRequest($"api/{Version}/pair-info");
            restRequest.Method = Method.GET;

            restRequest.AddQueryParameter("exchangePairName", filter.ExchangePairName);
            restRequest.AddQueryParameter("baseCurrency", filter.BaseCurrency);
            restRequest.AddQueryParameter("quoteCurrency", filter.QuoteCurrency);
            if(filter.Exchange.HasValue)
                restRequest.AddQueryParameter("exchange", filter.Exchange.Value.ToString());
            
            var response = await ExecuteRequestWithTimeOut(restRequest);
            return DeserializeResponse<Page<ArbitralPairInfo>,IPage<IArbitralPairInfo>>(response);
        }
    }
}