using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using Bitmex.Net.Client;
using Bitmex.Net.Client.Interfaces;
using Bitmex.Net.Client.Objects;
using Bitmex.Net.Client.Objects.Requests;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    internal class BitmexConnector : BaseConnector, IPublicConnector
    {
        private readonly IBitmexClient _bitmexClient;
        private readonly IDtoConverter _converter;

        public BitmexConnector([NotNull] IDtoConverter converter,
            IBitmexClient bitmexClient = null)
        {
            if (bitmexClient == null)
                _bitmexClient = new BitmexClient();
            else
                _bitmexClient = bitmexClient;

            _converter = converter;
        }

        async Task<long> IPublicConnector.GetServerTime(CancellationToken ct)
        {
            throw new NotSupportedException();
        }

        async Task<IEnumerable<IPairInfo>> IPublicConnector.GetPairsInfo(CancellationToken ct)
        {
            var response = await _bitmexClient.GetInstrumentsAsync(new BitmexRequestWithFilter().AddFilter("state","Open"), ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<Instrument>, IEnumerable<PairInfo>>(response.Data);
        }

        async Task<IEnumerable<IPairPrice>> IPublicConnector.GetPairPrices(CancellationToken ct)
        {
            var response = await _bitmexClient.GetInstrumentsAsync(new BitmexRequestWithFilter().AddFilter("state","Open"),ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<Instrument>, IEnumerable<PairPrice>>(response.Data);
        }

        public Task<IOrderBook> GetOrderBook(string symbol, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException("BitMex public order book");
        }
    }
}