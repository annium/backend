using System.Threading.Tasks;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class BodyExecutor : ITestExecutor
    {
        public uint Order { get; } = 4;

        private readonly MethodExecutor executor;

        private readonly ILogger logger;

        public BodyExecutor(
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
            if (test.Method == null)
                return Task.CompletedTask;

            logger.LogTrace($"Execute Body of {target.Test.DisplayName}.");

            return executor.ExecuteAsync(instance, test.Method, result);
        }
    }
}