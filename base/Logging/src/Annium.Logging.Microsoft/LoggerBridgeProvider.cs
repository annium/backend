using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;
using ILoggerFactory = Annium.Logging.Abstractions.ILoggerFactory;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridgeProvider : ILoggerProvider
    {
        private readonly ILogRouter _router;
        private readonly Abstractions.ILogger _logger;

        public LoggerBridgeProvider(
            ILogRouter router,
            ILoggerFactory loggerFactory
        )
        {
            _router = router;
            _logger = loggerFactory.GetLogger<LoggerBridge>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerBridge(_router, _logger, categoryName);
        }

        public void Dispose()
        {
        }
    }
}