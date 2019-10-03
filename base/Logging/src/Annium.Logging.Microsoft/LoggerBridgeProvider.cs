using System;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridgeProvider : ILoggerProvider
    {
        private readonly ILogRouter router;

        public LoggerBridgeProvider(
            ILogRouter router
        )
        {
            this.router = router;
        }

        public global::Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            var type = Type.GetType(categoryName) ?? typeof(LoggerBridgeProvider);

            return new LoggerBridge(router, type);
        }

        public void Dispose() { }
    }
}