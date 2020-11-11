using System;
using System.Linq;
using Annium.Logging.Abstractions;

namespace Annium.Extensions.Shell
{
    internal class Shell : IShell
    {
        private readonly ILogger<Shell> _logger;

        public Shell(
            ILogger<Shell> logger
        )
        {
            _logger = logger;
        }

        public IShellInstance Cmd(params string[] command)
        {
            if (command.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Shell command must be non-empty");

            return new ShellInstance(string.Join(' ', command), _logger);
        }
    }
}