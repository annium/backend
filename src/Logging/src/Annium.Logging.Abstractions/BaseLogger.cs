using System;
using NodaTime;

namespace Annium.Logging.Abstractions
{
    public abstract class BaseLogger<T> : ILogger<T> where T : class
    {
        protected static readonly DateTimeZone tz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

        protected readonly LoggerConfiguration configuration;

        private readonly Func<Instant> getInstant;

        protected BaseLogger(
            LoggerConfiguration configuration,
            Func<Instant> getInstant
        )
        {
            this.configuration = configuration;
            this.getInstant = getInstant;
        }

        public void Log(LogLevel level, string message)
        {
            if (level < configuration.LogLevel)
                return;

            LogMessage(getInstant(), level, message);
        }

        public void Trace(string message) => Log(LogLevel.Trace, message);

        public void Debug(string message) => Log(LogLevel.Debug, message);

        public void Info(string message) => Log(LogLevel.Info, message);

        public void Warn(string message) => Log(LogLevel.Warn, message);

        public void Error(Exception exception) => LogException(getInstant(), LogLevel.Error, exception);

        public void Error(string message) => LogMessage(getInstant(), LogLevel.Error, message);

        protected abstract void LogMessage(Instant instant, LogLevel level, string message);

        protected abstract void LogException(Instant instant, LogLevel level, Exception exception);
    }
}