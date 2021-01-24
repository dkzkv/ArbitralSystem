using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using Bittrex.Net;
using Bittrex.Net.Interfaces;
using Bittrex.Net.Objects;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    internal class BittrexConnector : BaseConnector, IPublicConnector
    {
        private readonly IBittrexClient _bittrexClient;
        private readonly IDtoConverter _converter;

        public BittrexConnector([NotNull] IDtoConverter converter,
            IBittrexClient bittrexClient = null)
        {
            if (bittrexClient == null)
                _bittrexClient = new BittrexClient();
            else
                _bittrexClient = bittrexClient;

            _converter = converter;
        }

        async Task<long> IPublicConnector.GetServerTime(CancellationToken ct)
        {
            var response = await _bittrexClient.GetSymbolsAsync(ct);
            ValidateResponse(response);
            var dateTimeString = response.ResponseHeaders.FirstOrDefault(o => o.Key == "Date")
                .Value?.FirstOrDefault();

            var dateTimeUtc = DateTime.Parse(dateTimeString).ToUniversalTime();

            return TimeHelper.DateTimeToTimeStamp(dateTimeUtc);
        }

        async Task<IEnumerable<IPairInfo>> IPublicConnector.GetPairsInfo(CancellationToken ct)
        {
            var response = await _bittrexClient.GetSymbolsAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<BittrexSymbol>, IEnumerable<PairInfo>>(response.Data);
        }

        async Task<IEnumerable<IPairPrice>> IPublicConnector.GetPairPrices(CancellationToken ct)
        {
            var response = await _bittrexClient.GetSymbolSummariesAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<BittrexSymbolSummary>, IEnumerable<PairPrice>>(response.Data);
        }
    }
}