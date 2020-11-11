using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class SetupExecutor : ITestExecutor
    {
        private readonly ILogger<SetupExecutor> _logger;

        public uint Order { get; } = 2;

        public SetupExecutor(
            ILogger<SetupExecutor> logger
        )
        {
            _logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            _logger.Trace($"Setup {target.Test.DisplayName}.");

            target.Init();

            return Task.CompletedTask;
        }
    }
}