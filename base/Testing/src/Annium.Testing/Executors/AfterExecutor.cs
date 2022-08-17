using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors;

public class AfterExecutor : ITestExecutor, ILogSubject<AfterExecutor>
{
    public uint Order { get; } = 5;
    public ILogger<AfterExecutor> Logger { get; }
    private readonly MethodExecutor _executor;

    public AfterExecutor(
        MethodExecutor executor,
        ILogger<AfterExecutor> logger
    )
    {
        _executor = executor;
        Logger = logger;
    }

    public Task ExecuteAsync(Target target)
    {
        var (instance, test, result) = target;
        if (test.After == null)
            return Task.CompletedTask;

        this.Log().Trace($"Execute After of {target.Test.DisplayName}.");

        return _executor.ExecuteAsync(instance, test.After, result);
    }
}