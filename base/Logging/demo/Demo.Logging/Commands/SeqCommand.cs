using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging.Abstractions;

namespace Demo.Logging.Commands;

internal class SeqCommand : AsyncCommand, ILogSubject<SeqCommand>
{
    public override string Id { get; } = "seq";
    public override string Description => $"test {Id} log handler";
    public ILogger<SeqCommand> Logger { get; }

    public SeqCommand(
        ILogger<SeqCommand> logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        this.Log().Info("demo");
        await Task.Delay(100, ct);
    }
}