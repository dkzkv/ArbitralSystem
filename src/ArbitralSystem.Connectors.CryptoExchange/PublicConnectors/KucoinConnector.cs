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
using Kucoin.Net;
using Kucoin.Net.Interfaces;
using Kucoin.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    internal class KucoinConnector : BaseConnector, IPublicConnector
    {
        private readonly IDtoConverter _converter;
        private readonly IKucoinClient _kucoinClient;

        public KucoinConnector([NotNull] IDtoConverter converter,
            IKucoinClient kucoinClient = null)
        {
            if (kucoinClient == null)
                _kucoinClient = new KucoinClient();
            else
                _kucoinClient = kucoinClient;

            _converter = converter;
        }

        async Task<long> IPublicConnector.GetServerTime(CancellationToken ct)
        {
            var response = await _kucoinClient.GetServerTimeAsync(ct);
            ValidateResponse(response);
            return TimeHelper.DateTimeToTimeStamp(response.Data.ToUniversalTime());
        }

        async Task<IEnumerable<IPairInfo>> IPublicConnector.GetPairsInfo(CancellationToken ct)
        {
            var response = await _kucoinClient.GetSymbolsAsync(ct:ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<KucoinSymbol>, IEnumerable<PairInfo>>(response.Data.Where(o=>o.EnableTrading));
        }

        async Task<IEnumerable<IPairPrice>> IPublicConnector.GetPairPrices(CancellationToken ct)
        {
            var response = await _kucoinClient.GetTickersAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<KucoinAllTick>, IEnumerable<PairPrice>>(response.Data.Data);
        }

        public Task<IOrderBook> GetOrderBook(string symbol, CancellationToken ct = default(CancellationToken))
        {
            throw new System.NotImplementedException("Kucoin public order book");
        }
    }
}