using System;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Logging.Shared
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