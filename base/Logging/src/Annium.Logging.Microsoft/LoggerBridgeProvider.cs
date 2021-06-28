using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridgeProvider : ILoggerProvider
    {
        private readonly ILogSentry _sentry;

        public LoggerBridgeProvider(
            ILogSentry sentry
        )
        {
            _sentry = sentry;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerBridge(_sentry, categoryName);
        }

        public void Dispose()
        {
        }
    }
}