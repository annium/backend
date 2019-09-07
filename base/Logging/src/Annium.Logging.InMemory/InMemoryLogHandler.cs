using System.Collections.Generic;
using Annium.Logging.Abstractions;

namespace Annium.Logging.InMemory
{
    public class InMemoryLogHandler : ILogHandler
    {
        public IReadOnlyList<LogMessage> Logs => logs;

        private readonly List<LogMessage> logs = new List<LogMessage>();

        public void Handle(LogMessage message) => logs.Add(message);
    }
}