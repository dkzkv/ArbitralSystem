using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using JetBrains.Annotations;
using Kraken.Net;
using Kraken.Net.Interfaces;
using Kraken.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    internal class KrakenConnector : BaseConnector , IPublicConnector
    {
        private readonly IDtoConverter _converter;
        private readonly IKrakenClient _krakenClient;

        public KrakenConnector([NotNull] IDtoConverter converter,
            IKrakenClient krakenClient = null)
        {
            if (krakenClient == null)
                _krakenClient = new KrakenClient();
            else
                _krakenClient = krakenClient;

            _converter = converter;
        }

        async Task<long> IPublicConnector.GetServerTime(CancellationToken ct)
        {
            var response = await _krakenClient.GetServerTimeAsync(ct);
            ValidateResponse(response);
            return TimeHelper.DateTimeToTimeStamp(response.Data.ToUniversalTime());
        }

        async Task<IEnumerable<IPairInfo>> IPublicConnector.GetPairsInfo(CancellationToken ct)
        {
            var response = await _krakenClient.GetSymbolsAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<KrakenSymbol>, IEnumerable<PairInfo>>(response.Data.Values);
        }
 
        async Task<IEnumerable<IPairPrice>> IPublicConnector.GetPairPrices(CancellationToken ct)
        {
            var pairsResponse = await _krakenClient.GetSymbolsAsync(ct);
            ValidateResponse(pairsResponse);
            var availableExchangePairs = pairsResponse.Data.Keys.Select(o => o.ToString()).ToArray();
            var response = _krakenClient.GetTickers(ct, availableExchangePairs);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<KeyValuePair<string,KrakenRestTick>>, IEnumerable<PairPrice>>(response.Data);
        }
    }
}