using System;
using Annium.Logging.Shared;
using Microsoft.Extensions.Logging;
using LogLevel = Annium.Logging.Abstractions.LogLevel;
using MicrosoftEventId = Microsoft.Extensions.Logging.EventId;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Annium.Logging.Microsoft;

internal class LoggingBridge : ILogger
{
    private readonly ILogSentryBridge _sentryBridge;
    private readonly string _source;

    public LoggingBridge(
        ILogSentryBridge sentryBridge,
        string source
    )
    {
        _sentryBridge = sentryBridge;
        _source = source;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => Disposable.Empty;

    public bool IsEnabled(MicrosoftLogLevel logLevel) => true;

    public void Log<TState>(
        MicrosoftLogLevel logLevel,
        MicrosoftEventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        _sentryBridge.Register<LoggingBridge>(
            null,
            string.Empty,
            string.Empty,
            0,
            Map(logLevel),
            _source,
            formatter(state, exception),
            exception,
            Array.Empty<object>()
        );
    }

    private LogLevel Map(MicrosoftLogLevel level)
    {
        switch (level)
        {
            case MicrosoftLogLevel.Trace:
                return LogLevel.Trace;
            case MicrosoftLogLevel.Debug:
                return LogLevel.Debug;
            case MicrosoftLogLevel.Information:
                return LogLevel.Info;
            case MicrosoftLogLevel.Warning:
                return LogLevel.Warn;
            case MicrosoftLogLevel.Error:
            case MicrosoftLogLevel.Critical:
                return LogLevel.Error;
            default:
                return LogLevel.None;
        }
    }
}