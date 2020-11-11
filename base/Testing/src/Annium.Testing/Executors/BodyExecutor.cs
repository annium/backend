using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class BodyExecutor : ITestExecutor
    {
        public uint Order { get; } = 4;

        private readonly MethodExecutor _executor;

        private readonly ILogger<BodyExecutor> _logger;

        public BodyExecutor(
            MethodExecutor executor,
            ILogger<BodyExecutor> logger
        )
        {
            _executor = executor;
            _logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var (instance, test, result) = target;
            if (test.Method == null)
                return Task.CompletedTask;

            _logger.Trace($"Execute Body of {target.Test.DisplayName}.");

            return _executor.ExecuteAsync(instance, test.Method, result);
        }
    }
}