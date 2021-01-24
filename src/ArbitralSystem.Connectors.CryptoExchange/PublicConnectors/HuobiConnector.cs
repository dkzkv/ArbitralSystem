using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Helpers;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using Huobi.Net;
using Huobi.Net.Interfaces;
using Huobi.Net.Objects;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    internal class HuobiConnector : BaseConnector , IPublicConnector
    {
        private readonly IDtoConverter _converter;
        private readonly IHuobiClient _huobiClient;

        public HuobiConnector([NotNull] IDtoConverter converter,
            IHuobiClient huobiClient = null)
        {
            if (huobiClient == null)
                _huobiClient = new HuobiClient();
            else
                _huobiClient = huobiClient;

            _converter = converter;
        }

        async Task<long> IPublicConnector.GetServerTime(CancellationToken ct)
        {
            var response = await _huobiClient.GetServerTimeAsync(ct);
            ValidateResponse(response);
            return TimeHelper.DateTimeToTimeStamp(response.Data.ToUniversalTime());
        }

        async Task<IEnumerable<IPairInfo>> IPublicConnector.GetPairsInfo(CancellationToken ct)
        {
            var response = await _huobiClient.GetSymbolsAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<HuobiSymbol>, IEnumerable<PairInfo>>(response.Data);
        }

        async Task<IEnumerable<IPairPrice>> IPublicConnector.GetPairPrices(CancellationToken ct)
        {
            var response = await _huobiClient.GetTickersAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<HuobiSymbolTick>, IEnumerable<PairPrice>>(response.Data.Ticks);
        }
    }
}