using System;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Logging.Abstractions
{
    public class InMemoryLogger<T> : BaseLogger<T> where T : class
    {
        public IReadOnlyList<ValueTuple<Instant, LogLevel, string>> Logs => logs;

        private readonly List<ValueTuple<Instant, LogLevel, string>> logs = new List<ValueTuple<Instant, LogLevel, string>>();

        public InMemoryLogger(
            LoggerConfiguration configuration,
            Func<Instant> getInstant
        ) : base(configuration, getInstant) { }

        protected override void LogException(Instant instant, LogLevel level, Exception exception)
        {
            logs.Add((instant, level, exception.Message));
        }

        protected override void LogMessage(Instant instant, LogLevel level, string message)
        {
            logs.Add((instant, level, message));
        }
    }
}