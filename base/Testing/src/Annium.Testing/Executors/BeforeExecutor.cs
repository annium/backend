using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class BeforeExecutor : ITestExecutor
    {
        public uint Order { get; } = 3;

        private readonly MethodExecutor _executor;

        private readonly ILogger<BeforeExecutor> _logger;

        public BeforeExecutor(
            MethodExecutor executor,
            ILogger<BeforeExecutor> logger
        )
        {
            _executor = executor;
            _logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var (instance, test, result) = target;
            if (test.Before == null)
                return Task.CompletedTask;

            _logger.Trace($"Execute Before of {target.Test.DisplayName}.");

            return _executor.ExecuteAsync(instance, test.Before, result);
        }
    }
}