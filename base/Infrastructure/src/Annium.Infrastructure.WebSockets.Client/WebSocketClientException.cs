using System;
using Annium.Data.Operations;

namespace Annium.Infrastructure.WebSockets.Client
{
    public class WebSocketClientException : Exception
    {
        public IResultBase Result { get; }

        public WebSocketClientException(IResultBase result)
        {
            Result = result;
        }
    }
}