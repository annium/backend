using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class SetupExecutor : ITestExecutor
    {
        private readonly ILogger<SetupExecutor> logger;

        public uint Order { get; } = 2;

        public SetupExecutor(
            ILogger<SetupExecutor> logger
        )
        {
            this.logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            logger.Trace($"Setup {target.Test.DisplayName}.");

            target.Init();

            return Task.CompletedTask;
        }
    }
}