using System.Threading.Tasks;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class BeforeExecutor : ITestExecutor
    {
        public uint Order { get; } = 3;

        private readonly MethodExecutor executor;

        private readonly ILogger logger;

        public BeforeExecutor(
            MethodExecutor executor,
            ILogger logger
        )
        {
            this.executor = executor;
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var(instance, test, result) = target;
            if (test.Before == null)
                return Task.CompletedTask;

            logger.LogTrace($"Execute Before of {target.Test.DisplayName}.");

            return executor.ExecuteAsync(instance, test.Before, result);
        }
    }
}