using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.Core.Common;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Helpers;
using ArbitralSystem.Domain.MarketInfo;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ArbitralSystem.Connectors.Core
{
    public abstract class BaseRestClient
    {
        private const int DefaultRequestTimeOutInMs = 10000;

        private readonly RestClient _client;
        private readonly ConnectionOptions _connectionOptions;

        public BaseRestClient(string baseUrl)
        {
            _client = BuildRestClient(baseUrl);
            _connectionOptions = new ConnectionOptions(DefaultRequestTimeOutInMs);
        }

        public BaseRestClient(string baseUrl, ConnectionOptions connectionOptions)
        {
            _client = BuildRestClient(baseUrl);
            _connectionOptions = connectionOptions;
        }

        private RestClient BuildRestClient(string baseUrl)
        {
            var client = new RestClient(baseUrl);
            client.AddDefaultHeader("Accept", "application/json");
            return client;
        }

        protected virtual IResponse<TResponse> DeserializeResponse<TResponse>(
            [NotNull] IResponse<IRestResponse> response)
        {
            return InnerDeserializeResponse(response, () => JsonConvert.DeserializeObject<TResponse>(response.Data.Content));
        }

        protected virtual IResponse<TOut> DeserializeResponse<TIn, TOut>(
            [NotNull] IResponse<IRestResponse> response) where TIn : TOut
        {
            return InnerDeserializeResponse(response, () => (TOut) JsonConvert.DeserializeObject<TIn>(response.Data.Content));
        }

        private IResponse<TResponse> InnerDeserializeResponse<TResponse>(
            [NotNull] IResponse<IRestResponse> response , Func<TResponse>  deserializer)
        {
            if (!IsResponseValid<TResponse>(response, out var errorResponse))
                return errorResponse;

            var restResponse = response.Data;
            try
            {
                var data = deserializer.Invoke();
                return new Response<TResponse>(data);
            }
            catch (Exception ex)
            {
                var exception =
                    new RestClientException($"Error while parsing response , Content : {restResponse.Content}", ex);
                return new Response<TResponse>(exception);
            }
        }

        protected virtual IResponse<TOut> DeserializeResponse<TIn, TOut>(
            [NotNull] IResponse<IRestResponse> response, [NotNull] string jObject) where TIn : TOut
        {
            return InnerDeserializeResponse(response, jObject, (innerJMessage) => (TOut) innerJMessage[jObject].ToObject<TIn>());
        }

        protected virtual IResponse<TResponse> DeserializeResponse<TResponse>(
            [NotNull] IResponse<IRestResponse> response, [NotNull] string jObject)
        {
            return InnerDeserializeResponse(response, jObject, (innerJMessage) => innerJMessage[jObject].ToObject<TResponse>());
        }

        private IResponse<TResponse> InnerDeserializeResponse<TResponse>(
            [NotNull] IResponse<IRestResponse> response, [NotNull] string jObject, Func<JObject, TResponse> deserializer)
        {
            if (!IsResponseValid<TResponse>(response, out var errorResponse))
                return errorResponse;
            
            var restResponse = response.Data;

            var jMessage = JObject.Parse(restResponse.Content);
            if (!jMessage.ContainsKey(jObject))
            {
                var message = $"Result key {jObject} not found and error key missed";
                var exception = new RestClientException($"{message}");
                return new Response<TResponse>(exception);
            }

            try
            {
                var objectMessage = deserializer.Invoke(jMessage);
                return new Response<TResponse>(objectMessage);
            }
            catch (Exception ex)
            {
                var message = $"Cannot deserialize response {restResponse.Content}";
                var exception = new RestClientException($"{message}", ex);
                return new Response<TResponse>(exception);
            }
        }
        
        private bool IsResponseValid<TResponse>([NotNull] IResponse<IRestResponse> response, out IResponse<TResponse> errorResponse)
        {
            errorResponse = null;
            if (!response.IsSuccess)
            {
                errorResponse = new Response<TResponse>(response.Exception);
                return false;
            }

            var restResponse = response.Data;
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                if (!restResponse.Content.IsValidJson())
                {
                    var exception =
                        new RestClientException($"Response content is not valid. Uri: {restResponse.ResponseUri}");
                    errorResponse = new Response<TResponse>(exception);
                    return false;
                }

                return true;
            }
            else
            {
                var logMessage = $"response status: {restResponse.Content}.";
                var exception = new UnexpectedRestClientException(logMessage, restResponse.ErrorException);
                errorResponse = new Response<TResponse>(exception);
                return false;
            }
        }

        protected async Task<IResponse<IRestResponse>> ExecuteRequestWithTimeOut(IRestRequest restRequest, int milliseconds)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(milliseconds));
                var response = await _client.ExecuteTaskAsync(restRequest, cts.Token);
                return new Response<IRestResponse>(response);
            }
            catch (TaskCanceledException ex)
            {
                var exception = new RestClientException($"Timout in {restRequest.Resource}", ex);
                return new Response<IRestResponse>(exception);
            }
        }

        protected async Task<IResponse<IRestResponse>> ExecuteRequestWithTimeOut(IRestRequest restRequest)
        {
            return await ExecuteRequestWithTimeOut(restRequest, _connectionOptions.DefaultTimeOutInMs);
        }
    }
}