using System;

namespace ArbitralSystem.Trading.SimpleTradingBot.Common.Exceptions
{
    public class StrategyException : Exception
    {
        public StrategyException(string message) : base(message) { }

        public StrategyException(string message, Exception inner) : base(message, inner) { }
    }
}