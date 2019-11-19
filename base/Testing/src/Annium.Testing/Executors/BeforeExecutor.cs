using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class BeforeExecutor : ITestExecutor
    {
        public uint Order { get; } = 3;

        private readonly MethodExecutor executor;

        private readonly ILogger<BeforeExecutor> logger;

        public BeforeExecutor(
            MethodExecutor executor,
            ILogger<BeforeExecutor> logger
        )
        {
            this.executor = executor;
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var (instance, test, result) = target;
            if (test.Before == null)
                return Task.CompletedTask;

            logger.Trace($"Execute Before of {target.Test.DisplayName}.");

            return executor.ExecuteAsync(instance, test.Before, result);
        }
    }
}