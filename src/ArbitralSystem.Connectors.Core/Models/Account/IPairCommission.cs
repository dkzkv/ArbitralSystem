using ArbitralSystem.Domain.MarketInfo;

namespace ArbitralSystem.Connectors.Core.Models.Account
{
    public interface IPairCommission : IExchange, IExchangePairName
    {
        decimal TakerPercent { get; }
        decimal MakerPercent { get; }
    }

    public interface IWithdrawCurrencyCommission : IExchange
    {
        string ChainName { get; }
        string Currency { get; }
        decimal? MinDepositAmount { get; }
        decimal MinWithdrawAmount { get; }
        decimal? MaxWithdrawAmount { get; }
        decimal? WithdrawQuotaPerDay { get; }
        decimal Commission { get; }
        int? Precision { get; }
    }
}