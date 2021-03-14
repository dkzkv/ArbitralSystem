using System.Collections.Generic;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.CoinEx.Models;
using ArbitralSystem.Connectors.CoinEx.Models.Auxiliary;
using ArbitralSystem.Connectors.Core;
using ArbitralSystem.Connectors.Core.Common;
using ArbitralSystem.Domain.MarketInfo;
using RestSharp;

namespace ArbitralSystem.Connectors.CoinEx
{
    public class CoinExConnector : BaseRestClient, ICoinExConnector
    {
        private const string BaseUrl = "https://api.coinex.com/";
        private const string Version = "v1";

        public CoinExConnector()
            : base(BaseUrl)
        {
        }
        
        public CoinExConnector(ConnectionOptions connectionOptions)
            : base(BaseUrl, connectionOptions)
        {
        }
        
        public async Task<IResponse<IEnumerable<string>>> GetMarketList()
        {
            var restRequest = new RestRequest($"{Version}/market/list");
            restRequest.Method = Method.GET;

            var response = await ExecuteRequestWithTimeOut(restRequest);
            return DeserializeResponse<IEnumerable<string>>(response, "data");
        }
        
        public async Task<IResponse<IMarketInfo>> GetMarketSingleInfo(string symbol)
        {
            var restRequest = new RestRequest($"{Version}/market/detail");
            restRequest.Method = Method.GET;

            restRequest.AddQueryParameter("market", symbol);

            var response = await ExecuteRequestWithTimeOut(restRequest);
            return DeserializeResponse<MarketInfo,IMarketInfo>(response, "data");
        }
    }
}