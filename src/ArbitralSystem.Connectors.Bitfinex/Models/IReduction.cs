namespace ArbitralSystem.Connectors.Bitfinex.Models
{
    public interface IReduction
    {
        string ReductionCurrency { get; }
        string OriginalCurrency { get; }
    }
}