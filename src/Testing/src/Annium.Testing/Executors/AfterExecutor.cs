using System.Threading.Tasks;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class AfterExecutor : ITestExecutor
    {
        public uint Order { get; } = 5;

        private readonly MethodExecutor executor;

        private readonly ILogger logger;

        public AfterExecutor(
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
            if (test.After == null)
                return Task.CompletedTask;

            logger.LogTrace($"Execute After of {target.Test.DisplayName}.");

            return executor.ExecuteAsync(instance, test.After, result);
        }
    }
}