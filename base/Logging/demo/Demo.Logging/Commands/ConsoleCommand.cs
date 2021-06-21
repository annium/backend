using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace Demo.Logging.Commands
{
    internal class ConsoleCommand : AsyncCommand, ILogSubject
    {
        public override string Id { get; } = "console";
        public override string Description => $"test {Id} log handler";
        public ILogger Logger { get; }

        public ConsoleCommand(
            ILogger<ConsoleCommand> logger
        )
        {
            Logger = logger;
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            this.Log().Debug("demo");
            await Task.Delay(100, ct);
        }
    }
}