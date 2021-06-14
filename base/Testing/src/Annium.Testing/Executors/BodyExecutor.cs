using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class BodyExecutor : ITestExecutor, ILogSubject
    {
        public uint Order => 4;
        public ILogger Logger { get; }
        private readonly MethodExecutor _executor;

        public BodyExecutor(
            MethodExecutor executor,
            ILogger<BodyExecutor> logger
        )
        {
            _executor = executor;
            Logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var (instance, test, result) = target;
            if (test.Method == null)
                return Task.CompletedTask;

            this.Trace($"Execute Body of {target.Test.DisplayName}.");

            return _executor.ExecuteAsync(instance, test.Method, result);
        }
    }
}