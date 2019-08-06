using System.Threading.Tasks;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class SetupExecutor : ITestExecutor
    {
        private readonly ILogger logger;

        public uint Order { get; } = 2;

        public SetupExecutor(
            ILogger logger
        )
        {
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            logger.LogTrace($"Setup {target.Test.DisplayName}.");

            target.Init();

            return Task.CompletedTask;
        }
    }
}