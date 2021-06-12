using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridgeProvider : ILoggerProvider
    {
        private readonly ILogRouter _router;

        public LoggerBridgeProvider(
            ILogRouter router
        )
        {
            _router = router;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerBridge(_router, categoryName);
        }

        public void Dispose()
        {
        }
    }
}