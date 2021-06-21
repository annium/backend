using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Logging.Shared;

namespace Annium.Logging.InMemory
{
    public class InMemoryLogHandler : ILogHandler
    {
        public IReadOnlyList<LogMessage> Logs => _logs;

        private readonly List<LogMessage> _logs = new();

        public void Handle(LogMessage message)
        {
            _logs.Add(message);
        }
    }
}