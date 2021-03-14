using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Bitfinex.Models;
using ArbitralSystem.Connectors.Bitfinex.Models.Auxiliary;
using ArbitralSystem.Connectors.Core;
using ArbitralSystem.Connectors.Core.Common;
using RestSharp;

namespace ArbitralSystem.Connectors.Bitfinex
{
    public class BitfinexConnector : BaseRestClient , IBitfinexConnector
    {
        private const string BaseUrl = "https://api-pub.bitfinex.com/";
        private const string Version2 = "v2";
        
        public BitfinexConnector() : base(BaseUrl)
        {
        }
        
        public BitfinexConnector(ConnectionOptions connectionOptions) : base(BaseUrl, connectionOptions)
        {
        }
        
        public async Task<IResponse<IEnumerable<IReduction>>> GetCurrencyReductionsAsync()
        {
            var restRequest = new RestRequest($"{Version2}/conf/pub:map:currency:sym");
            restRequest.Method = Method.GET;
            
            var response = await ExecuteRequestWithTimeOut(restRequest);
            var rawResponse = DeserializeResponse<IEnumerable<IEnumerable<IEnumerable<string>>>>(response);

            if (!rawResponse.IsSuccess)
                return new Response<IEnumerable<IReduction>>(rawResponse.Exception);
            
            return new Response<IEnumerable<IReduction>>(rawResponse.Data
                .First()
                .Select(o => o.ToArray()).Select(o => new Reduction(o[0], o[1]) as IReduction));
            
        }
    }
}