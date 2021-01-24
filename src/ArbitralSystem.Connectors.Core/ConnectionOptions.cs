using System;

namespace ArbitralSystem.Connectors.Core
{
    public class ConnectionOptions
    {
        public ConnectionOptions(int defaultTimeOutInMs)
        {
            if(defaultTimeOutInMs <= 0)
                throw new ArgumentException("Default timeout should be positive value.");
            DefaultTimeOutInMs = defaultTimeOutInMs;
        }
        
        public int DefaultTimeOutInMs { get; }
    }
}
