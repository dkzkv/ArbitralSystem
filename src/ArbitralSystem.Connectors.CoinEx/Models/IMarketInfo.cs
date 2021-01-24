namespace ArbitralSystem.Connectors.CoinEx.Models
{
    public interface IMarketInfo
    {
        decimal FreeRate { get; }
        string PricingName { get; }
        string TradingName { get; }
        public string Name { get; }
        decimal MinAmount { get; }
        int TradingPrecision { get; }
        decimal MakerFee { get; }
        int PricingPrecision { get; }
    }
}