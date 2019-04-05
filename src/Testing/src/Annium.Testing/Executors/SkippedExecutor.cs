using System.Threading.Tasks;
using Annium.Testing.Elements;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class SkippedExecutor : ITestExecutor
    {
        private readonly ILogger logger;

        public uint Order { get; } = 1;

        public SkippedExecutor(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            if (target.Test.IsSkipped)
            {
                target.Result.Outcome = TestOutcome.Skipped;

                logger.LogTrace($"Skip {target.Test.DisplayName}.");
            }

            return Task.CompletedTask;
        }
    }
}