namespace ArbitralSystem.Common.Helpers
{
    public static class Calculator
    {
        public static decimal ComputePercent(decimal x1, decimal x2) => (x1 - x2) / x2 * 100;
    }
}