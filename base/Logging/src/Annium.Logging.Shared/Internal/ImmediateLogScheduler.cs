using System;

namespace Annium.Logging.Shared.Internal
{
    internal class ImmediateLogScheduler : ILogScheduler
    {
        public Func<LogMessage, bool> Filter { get; }
        private readonly ILogHandler _handler;

        public ImmediateLogScheduler(
            Func<LogMessage, bool> filter,
            ILogHandler handler
        )
        {
            Filter = filter;
            _handler = handler;
        }

        public void Handle(LogMessage message)
        {
            _handler.Handle(message);
        }
    }
}