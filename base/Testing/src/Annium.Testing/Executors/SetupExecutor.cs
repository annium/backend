using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors;

public class SetupExecutor : ITestExecutor, ILogSubject<SetupExecutor>
{
    public ILogger<SetupExecutor> Logger { get; }

    public uint Order { get; } = 2;

    public SetupExecutor(
        ILogger<SetupExecutor> logger
    )
    {
        Logger = logger;
    }

    public Task ExecuteAsync(Target target)
    {
        this.Log().Trace($"Setup {target.Test.DisplayName}.");

        target.Init();

        return Task.CompletedTask;
    }
}