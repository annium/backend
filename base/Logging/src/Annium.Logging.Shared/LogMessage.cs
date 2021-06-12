using System;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Logging.Shared
{
    public class LogMessage
    {
        public Instant Instant { get; }
        public LogLevel Level { get; }
        public string Source { get; }
        public int ThreadId { get; }
        public string Message { get; }
        public Exception? Exception { get; }
        public object? Subject { get; }
        public object? Data { get; }
        public bool WithTrace { get; }
        public string Caller { get; }
        public string Member { get; }
        public int Line { get; }

        internal LogMessage(
            Instant instant,
            LogLevel level,
            string source,
            int threadId,
            string message,
            Exception? exception,
            object? subject,
            object? data,
            bool withTrace,
            string caller,
            string member,
            int line
        )
        {
            Instant = instant;
            Level = level;
            Source = source;
            ThreadId = threadId;
            Message = message;
            Exception = exception;
            Subject = subject;
            Data = data;
            WithTrace = withTrace;
            Caller = caller;
            Member = member;
            Line = line;
        }
    }
}