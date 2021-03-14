using ArbitralSystem.Connectors.Core.Models.Account;
using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.CryptoExchange.Models.Auxiliary
{
    internal class WithdrawCurrencyCommission : IWithdrawCurrencyCommission
    {
        public Exchange Exchange { get; set; }

        public string ChainName { get; set; }
        public string Currency { get; set; }
        public decimal? MinDepositAmount { get; set; }
        public decimal MinWithdrawAmount { get; set; }
        public decimal? MaxWithdrawAmount { get; set; }
        public decimal? WithdrawQuotaPerDay { get; set; }
        public decimal Commission { get; set; }
        public int? Precision { get; set; }
    }
}