using System;
using Annium.Logging.Abstractions;
using MicrosoftEventId = Microsoft.Extensions.Logging.EventId;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Annium.Logging.Microsoft
{
    internal class LoggerBridge : global::Microsoft.Extensions.Logging.ILogger
    {
        private readonly ILogRouter router;
        private readonly Type source;

        public LoggerBridge(
            ILogRouter router,
            Type source
        )
        {
            this.router = router;
            this.source = source;
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
            router.Send(Map(logLevel), source, formatter(state, exception), exception);
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