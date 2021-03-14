using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Logger;
using ArbitralSystem.Connectors.CoinEx;
using ArbitralSystem.Connectors.CoinEx.Models;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using CoinEx.Net;
using CoinEx.Net.Interfaces;
using CoinEx.Net.Objects;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    internal class CoinExConnector : BaseConnector, IPublicConnector
    {
        private readonly ICoinExConnector _coinExConnector;
        private readonly ICoinExClient _coinExClient;
        private readonly IDtoConverter _converter;
        private readonly ILogger _logger;

        public CoinExConnector(ICoinExConnector coinExConnector,
            IDtoConverter converter,
            ILogger logger,
            ICoinExClient coinExClient = null)
        {
            _coinExConnector = coinExConnector;
            _converter = converter;
            _logger = logger;

            if (coinExClient == null)
                _coinExClient = new CoinExClient();
            else
                _coinExClient = coinExClient;
        }

        Task<long> IPublicConnector.GetServerTime(CancellationToken ct)
        {
            throw new NotSupportedException("CoinEx server time not supported");
        }

        async Task<IEnumerable<IPairInfo>> IPublicConnector.GetPairsInfo(CancellationToken ct)
        {
            var pairs = await _coinExConnector.GetMarketList();
            var pairsForResult = new List<PairInfo>();

            if (pairs.IsSuccess)
            {
                _logger.Debug("List of pairs for CoinEx successfully received");
                foreach (var pair in pairs.Data)
                    if (!pair.Any(o => char.IsDigit(o)))
                    {
                        var rawPairInfo = await _coinExConnector.GetMarketSingleInfo(pair);
                        if (rawPairInfo.IsSuccess)
                        {
                            _logger.Debug("Pair information received: {@rawPairInfo}", rawPairInfo.Data);
                            pairsForResult.Add(_converter.Convert<IMarketInfo, PairInfo>(rawPairInfo.Data));
                        }
                    }
            }
            else
            {
                throw pairs.Exception;
            }

            return pairsForResult;
        }

        async Task<IEnumerable<IPairPrice>> IPublicConnector.GetPairPrices(CancellationToken ct)
        {
            var response = await _coinExClient.GetSymbolStatesAsync(ct);
            ValidateResponse(response);
            return _converter.Convert<IEnumerable<KeyValuePair<string, CoinExSymbolStateData>>,
                IEnumerable<PairPrice>>(response.Data.Tickers.ToArray());
        }

        public Task<IOrderBook> GetOrderBook(string symbol, CancellationToken ct = default(CancellationToken))
        {
            throw new NotImplementedException("CoinEx public order book");
        }
    }
}