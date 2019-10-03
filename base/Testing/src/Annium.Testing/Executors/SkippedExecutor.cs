using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors
{
    public class SkippedExecutor : ITestExecutor
    {
        private readonly ILogger<SkippedExecutor> logger;

        public uint Order { get; } = 1;

        public SkippedExecutor(
            ILogger<SkippedExecutor> logger
        )
        {
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            if (target.Test.IsSkipped)
            {
                target.Result.Outcome = TestOutcome.Skipped;

                logger.Trace($"Skip {target.Test.DisplayName}.");
            }

            return Task.CompletedTask;
        }
    }
}