using ArbitralSystem.Connectors.Core.Models;

namespace ArbitralSystem.Connectors.Bitfinex.Models.Auxiliary
{
    internal class Reduction : IReduction
    {
        public Reduction(string reductionCurrency, string originalCurrency)
        {
            ReductionCurrency = reductionCurrency;
            OriginalCurrency = originalCurrency;
        }

        public string ReductionCurrency { get; }
        public string OriginalCurrency { get; }
    }
}