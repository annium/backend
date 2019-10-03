using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class AfterExecutor : ITestExecutor
    {
        public uint Order { get; } = 5;

        private readonly MethodExecutor executor;

        private readonly ILogger<AfterExecutor> logger;

        public AfterExecutor(
            MethodExecutor executor,
            ILogger<AfterExecutor> logger
        )
        {
            this.executor = executor;
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var(instance, test, result) = target;
            if (test.After == null)
                return Task.CompletedTask;

            logger.Trace($"Execute After of {target.Test.DisplayName}.");

            return executor.ExecuteAsync(instance, test.After, result);
        }
    }
}