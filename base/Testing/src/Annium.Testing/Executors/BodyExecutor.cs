using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class BodyExecutor : ITestExecutor
    {
        public uint Order { get; } = 4;

        private readonly MethodExecutor executor;

        private readonly ILogger<BodyExecutor> logger;

        public BodyExecutor(
            MethodExecutor executor,
            ILogger<BodyExecutor> logger
        )
        {
            this.executor = executor;
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var(instance, test, result) = target;
            if (test.Method == null)
                return Task.CompletedTask;

            logger.Trace($"Execute Body of {target.Test.DisplayName}.");

            return executor.ExecuteAsync(instance, test.Method, result);
        }
    }
}