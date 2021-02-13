using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArbitralSystem.Common.Validation;
using ArbitralSystem.Connectors.Core.Models;
using ArbitralSystem.Connectors.Core.Models.Account;
using ArbitralSystem.Connectors.Core.PrivateConnectors;
using ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary;
using ArbitralSystem.Connectors.CryptoExchange.PublicConnectors;
using ArbitralSystem.Domain.MarketInfo;
using Binance.Net;
using Binance.Net.Interfaces;
using JetBrains.Annotations;

namespace ArbitralSystem.Connectors.CryptoExchange.PrivateConnectors
{
    internal class BinanceExtraConnector : BaseConnector , IExtraConnector
    {
        private readonly IBinanceClient _binanceClient;
        
        public BinanceExtraConnector([NotNull] ICredentials credentials, IBinanceClient binanceClient = null)
        {
            Preconditions.CheckNotNull(credentials);
            
            if (binanceClient == null)
                _binanceClient = new BinanceClient();
            else
                _binanceClient = binanceClient;
            
            _binanceClient.SetApiCredentials(credentials.ApiKey, credentials.SecretKey);
        }
        
        async Task<IPairCommission> IExtraConnector.GetFeeRateAsync(string exchangePairName, CancellationToken token)
        {
            var exInfo = await _binanceClient.Spot.System.GetExchangeInfoAsync(token);
            ValidateResponse(exInfo);
            
            if(exInfo.Data.Symbols.All(o => o.Name != exchangePairName))
                throw new ArgumentException($"There is no pair: {exchangePairName}");
            
            var accInfo = await _binanceClient.General.GetAccountInfoAsync(ct: token);
            ValidateResponse(accInfo);
            return new PairCommission()
            {
                Exchange = Exchange.Binance,
                ExchangePairName = exchangePairName,
                MakerPercent = accInfo.Data.MakerCommission / 100,
                TakerPercent =  accInfo.Data.TakerCommission / 100,
            };
        }

        async Task<IEnumerable<IWithdrawCurrencyCommission>> IExtraConnector.GetWithdrawCommissionAsync(string currency, CancellationToken token)
        {
            var coinsResult = await _binanceClient.General.GetUserCoinsAsync(ct: token);
            ValidateResponse(coinsResult);
            var coinInfo = coinsResult.Data.FirstOrDefault(o => string.Equals(o.Coin, currency, StringComparison.InvariantCultureIgnoreCase));
            if(coinInfo == null)
                throw new ArgumentException($"There is no such currency: {currency}");
            return coinInfo.NetworkList.Select(o => new WithdrawCurrencyCommission()
            {
                ChainName = o.Name,
                Exchange = Exchange.Binance,
                Currency = coinInfo.Coin,
                Commission = o.WithdrawFee,
                MaxWithdrawAmount = null,
                MinDepositAmount = null,
                MinWithdrawAmount = o.WithdrawMin,
                Precision = null,
                WithdrawQuotaPerDay = null
            });
        }
    }
}