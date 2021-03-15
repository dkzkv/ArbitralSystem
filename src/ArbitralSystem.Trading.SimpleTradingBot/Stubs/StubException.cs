using System;

namespace ArbitralSystem.Trading.SimpleTradingBot.Stubs
{
    internal class StubException : Exception
    {
        public StubException(string message) : base(message)
        {
        }

        public StubException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}