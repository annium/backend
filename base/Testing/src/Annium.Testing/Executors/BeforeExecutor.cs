using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors;

public class BeforeExecutor : ITestExecutor, ILogSubject
{
    public ILogger Logger { get; }
    public uint Order => 3;
    private readonly MethodExecutor _executor;

    public BeforeExecutor(
        MethodExecutor executor,
        ILogger logger
    )
    {
        _executor = executor;
        Logger = logger;
    }

    public Task ExecuteAsync(Target target)
    {
        var (instance, test, result) = target;
        if (test.Before == null)
            return Task.CompletedTask;

        this.Log().Trace($"Execute Before of {target.Test.DisplayName}.");

        return _executor.ExecuteAsync(instance, test.Before, result);
    }
}