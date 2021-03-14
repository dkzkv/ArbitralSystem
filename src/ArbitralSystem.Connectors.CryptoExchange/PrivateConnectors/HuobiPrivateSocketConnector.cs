using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Converters;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using CryptoExchange.Net.Authentication;
using Huobi.Net;
using Huobi.Net.Interfaces;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class HuobiPrivateSocketConnector : BasePrivateSocketConnector, IPrivateSocketConnector
    {
        private readonly IHuobiSocketClient _huobiSocketClient;
        private readonly IConverter _converter;
        private readonly long _accountId;
        
        public HuobiPrivateSocketConnector([NotNull] IPrivateExchangeSettings credentials, IConverter converter)
        {
            Preconditions.CheckNotNull(credentials, converter);
            _huobiSocketClient = new HuobiSocketClient(new HuobiSocketClientOptions
                {ApiCredentials = new ApiCredentials(credentials.ApiKey, credentials.SecretKey)});
            _accountId = credentials.AccountId ?? throw new NullReferenceException(nameof(credentials.AccountId));
            _converter = converter;
        }


        public async Task SubscribeToBalanceUpdatesAsync(CancellationToken ct = default)
        {
            
            
            await _huobiSocketClient.SubscribeToAccountUpdatesAsync(update =>
            {
                //update.
            });
        }

        public async Task SubscribeToOrdersUpdatesAsync(string exchangePairName, CancellationToken ct = default)
        {
            await _huobiSocketClient.SubscribeToOrderDetailsUpdatesAsync(exchangePairName, update =>
            {
                
            });
        }


        public void Dispose()
        {
            _huobiSocketClient?.Dispose();
        }
    }
}