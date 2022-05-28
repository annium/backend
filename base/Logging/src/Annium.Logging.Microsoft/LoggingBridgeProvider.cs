using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft;

internal class LoggingBridgeProvider : ILoggerProvider
{
    private readonly ILogSentryBridge _sentryBridge;

    public LoggingBridgeProvider(
        ILogSentryBridge sentryBridge
    )
    {
        _sentryBridge = sentryBridge;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LoggingBridge(_sentryBridge, categoryName);
    }

    public void Dispose()
    {
    }
}