using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace Demo.Logging.Commands;

internal class InMemoryCommand : AsyncCommand, ILogSubject<InMemoryCommand>
{
    public override string Id { get; } = "in-memory";
    public override string Description => $"test {Id} log handler";
    public ILogger<InMemoryCommand> Logger { get; }

    public InMemoryCommand(
        ILogger<InMemoryCommand> logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        this.Log().Trace("demo");
        await Task.Delay(100, ct);
    }
}