using System;
using NodaTime;

namespace Annium.Logging.Abstractions
{
    public class LogMessage
    {
        public Instant Instant { get; }
        public LogLevel Level { get; }
        public Type Source { get; }
        public string Message { get; }
        public Exception? Exception { get; }

        internal LogMessage(
            Instant instant,
            LogLevel level,
            Type source,
            string message,
            Exception? exception
        )
        {
            Instant = instant;
            Level = level;
            Source = source;
            Message = message;
            Exception = exception;
        }
    }
}