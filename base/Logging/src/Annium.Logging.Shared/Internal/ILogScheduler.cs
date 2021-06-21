using System;

namespace Annium.Logging.Shared.Internal
{
    public interface ILogScheduler
    {
        Func<LogMessage, bool> Filter { get; }
        void Handle(LogMessage message);
    }
}