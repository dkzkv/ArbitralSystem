using System;

namespace ArbitralSystem.Connectors.Core.Exceptions
{
    public class RestClientException : Exception
    {
        public RestClientException(string message) : base(message) { }

        public RestClientException(string message, Exception inner) : base(message, inner) { }

    }
}
