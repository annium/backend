using System;
using Annium.Logging.Abstractions;
using Annium.Logging.Shared;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using MicrosoftEventId = Microsoft.Extensions.Logging.EventId;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridge : ILogger
    {
        private readonly ILogRouter _router;
        private readonly string _source;

        public LoggerBridge(
            ILogRouter router,
            string source
        )
        {
            _router = router;
            _source = source;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(MicrosoftLogLevel logLevel) => true;

        public void Log<TState>(
            MicrosoftLogLevel logLevel,
            MicrosoftEventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter
        )
        {
            _router.Send(
                Map(logLevel),
                _source,
                formatter(state, exception),
                exception,
                null,
                null,
                false,
                string.Empty,
                string.Empty,
                0
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
}