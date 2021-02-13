using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Connectors.Core.Exceptions;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Account;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary;
using ArbitralSystem.Domain.MarketInfo;
using Huobi.SDK.Core;
using Huobi.SDK.Core.Client;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    public class HuobiExtraConnector : IExtraConnector
    {
        private readonly OrderClient _huobiOrderClient;
        private readonly CommonClient _huobiCommonClient;

        public HuobiExtraConnector(ICredentials credentials)
        {
            _huobiOrderClient = new OrderClient(credentials.ApiKey, credentials.SecretKey);
            _huobiCommonClient = new CommonClient();
        }

        async Task<IPairCommission> IExtraConnector.GetFeeRateAsync(string exchangePairName, CancellationToken token)
        {
            var request = new GetRequest()
                .AddParam("symbols", exchangePairName);
            var result = await _huobiOrderClient.GetTransactFeeRateAsync(request);
            if (result.code == 200)
            {
                var rawFee = result.data.First(o => o.symbol == exchangePairName);
                return new PairCommission()
                {
                    Exchange = Exchange.Huobi,
                    ExchangePairName = rawFee.symbol,
                    MakerPercent = decimal.Parse(rawFee.actualMakerRate) * 100,
                    TakerPercent = decimal.Parse(rawFee.actualTakerRate) * 100,
                };
            }

            throw new RestClientException(result.message);
        }

        async Task<IEnumerable<IWithdrawCurrencyCommission>> IExtraConnector.GetWithdrawCommissionAsync(string currency, CancellationToken token)
        {
            var result = await _huobiCommonClient.GetCurrencyAsync(currency, false);
            if (result.code == 200)
            {
                return result.data.First(o => o.currency == currency).chains.Select(o =>
                    new WithdrawCurrencyCommission()
                    {
                        ChainName = o.baseChainProtocol ?? o.chain.ToUpper(),
                        Exchange = Exchange.Huobi,
                        Currency = currency,
                        MinDepositAmount = decimal.Parse(o.minDepositAmt),
                        MinWithdrawAmount = decimal.Parse(o.minWithdrawAmt),
                        MaxWithdrawAmount = decimal.Parse(o.maxWithdrawAmt),
                        WithdrawQuotaPerDay = string.IsNullOrEmpty(o.withdrawQuotaPerDay)
                            ? null
                            : (decimal?) decimal.Parse(o.withdrawQuotaPerDay),
                        Commission = decimal.Parse(o.transactFeeWithdraw),
                        Precision = o.withdrawPrecision
                    });
            }

            throw new RestClientException(result.message);
        }
    }
}