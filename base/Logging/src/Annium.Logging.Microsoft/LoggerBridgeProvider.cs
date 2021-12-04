using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;

namespace Annium.Logging.Microsoft;

internal class LoggerBridgeProvider : ILoggerProvider
{
    private readonly ILogSentryBridge _sentryBridge;

    public LoggerBridgeProvider(
        ILogSentryBridge sentryBridge
    )
    {
        _sentryBridge = sentryBridge;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new LoggerBridge(_sentryBridge, categoryName);
    }

    public void Dispose()
    {
    }
}