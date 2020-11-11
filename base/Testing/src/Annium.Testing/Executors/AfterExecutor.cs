using System.Threading.Tasks;
using Annium.Logging.Abstractions;

namespace Annium.Testing.Executors
{
    public class AfterExecutor : ITestExecutor
    {
        public uint Order { get; } = 5;

        private readonly MethodExecutor _executor;

        private readonly ILogger<AfterExecutor> _logger;

        public AfterExecutor(
            MethodExecutor executor,
            ILogger<AfterExecutor> logger
        )
        {
            _executor = executor;
            _logger = logger;
        }

        public Task ExecuteAsync(Target target)
        {
            var (instance, test, result) = target;
            if (test.After == null)
                return Task.CompletedTask;

            _logger.Trace($"Execute After of {target.Test.DisplayName}.");

            return _executor.ExecuteAsync(instance, test.After, result);
        }
    }
}