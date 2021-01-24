using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Bitfinex;
using ArbitralSystem.Connectors.Bitfinex.Models;
using ArbitralSystem.Connectors.Core.Common;
using ArbitralSystem.Connectors.Core.Converters;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.PublicConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models;
using ArbitralSystem.Domain.MarketInfo;
using Bitfinex.Net;
using Bitfinex.Net.Interfaces;
using Bitfinex.Net.Objects;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PublicConnectors
{
    public class BitfinexConnector : BaseConnector, IPublicConnector
    {
        private readonly IBitfinexConnector _bitfinexConnector;
        private readonly IBitfinexClient _bitfinexClient;
        private readonly IDtoConverter _converter;

        public BitfinexConnector([NotNull] IBitfinexConnector bitfinexConnector,
            [NotNull] IDtoConverter converter,
            IBitfinexClient bitfinexClient = null)
        {
            if (bitfinexClient == null)
                _bitfinexClient = new BitfinexClient();
            else
                _bitfinexClient = bitfinexClient;
            
            _bitfinexConnector = bitfinexConnector;
            _converter = converter;
        }
        public async Task<long> GetServerTime(CancellationToken ct = default(CancellationToken))
        {
            var response = await _bitfinexClient.PingAsync(ct);
            ValidateResponse(response);
            return response.Data;
        }

        public async Task<IEnumerable<IPairInfo>> GetPairsInfo(CancellationToken ct = default(CancellationToken))
        {
            IResponse<IEnumerable<IReduction>> reductions = await _bitfinexConnector.GetCurrencyReductionsAsync();
            ValidateResponse(reductions);
            
            var rawSymbols = await _bitfinexClient.GetSymbolsAsync(ct);
            ValidateResponse(rawSymbols);
            
            var splitSymbols = SplitRawSymbols(rawSymbols.Data);
            return PreparePairInfo(splitSymbols, reductions.Data.ToArray());
        }

        public async Task<IEnumerable<IPairPrice>> GetPairPrices(CancellationToken ct = default(CancellationToken))
        {
            var tickers = await _bitfinexClient.GetTickerAsync(ct, "ALL");
            ValidateResponse(tickers);
            return _converter.Convert<IEnumerable<BitfinexSymbolOverview>, IEnumerable<IPairPrice>>(tickers.Data);
        }

        private IEnumerable<IPairInfo> PreparePairInfo(IEnumerable<(string RawBase, string RawQuote)> splitSymbols, IReduction[] reductions)
        {
            foreach (var splitSymbol in splitSymbols)
            {
                var baseCurrency = reductions.Any(o => splitSymbol.RawBase == o.ReductionCurrency)
                    ? reductions.First(o => splitSymbol.RawBase == o.ReductionCurrency).OriginalCurrency
                    : splitSymbol.RawBase;
                
                var quoteCurrency = reductions.Any(o => splitSymbol.RawQuote == o.ReductionCurrency)
                    ? reductions.First(o => splitSymbol.RawQuote == o.ReductionCurrency).OriginalCurrency
                    : splitSymbol.RawQuote;
                
                yield return new PairInfo()
                {
                    Exchange = Exchange.Bitfinex,
                    BaseCurrency = baseCurrency,
                    QuoteCurrency = quoteCurrency,
                    ExchangePairName = $"t{splitSymbol.RawBase.ToUpper()}{splitSymbol.RawQuote.ToUpper()}"
                };
            }
        }
        
        private IEnumerable<(string RawBase, string RawQuote)> SplitRawSymbols(IEnumerable<string> rawSymbols)
        {
            foreach (var rawSymbol in rawSymbols.Select(o=>o.ToUpper()))
            {
                if (rawSymbol.Contains(':') && rawSymbol.Count() > 6)
                {
                    var parts = rawSymbol.Split(':');
                    yield return (parts[0], parts[1]);
                }
                else if (rawSymbol.Count() == 6)
                {
                    yield return (rawSymbol.Substring(0,3), rawSymbol.Substring(3));
                }
                else
                {
                    throw new RestClientException("Bitfinex wrong symbol parsing.");
                }
            }
        }
    }
}