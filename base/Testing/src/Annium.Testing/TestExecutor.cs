using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;
using Annium.Testing.Executors;

namespace Annium.Testing
{
    public class TestExecutor
    {
        private readonly TestingConfiguration _cfg;

        private readonly IServiceProvider _provider;

        private readonly PipelineExecutor _executor;

        private readonly ILogger<TestExecutor> _logger;

        public TestExecutor(
            TestingConfiguration cfg,
            IServiceProvider provider,
            PipelineExecutor executor,
            ILogger<TestExecutor> logger
        )
        {
            _cfg = cfg;
            _provider = provider;
            _executor = executor;
            _logger = logger;
        }

        public async Task RunTestsAsync(IEnumerable<Test> tests, Action<Test, TestResult> handleResult)
        {
            _logger.Debug("Start tests execution");

            var concurrency = Environment.ProcessorCount;

            using var semaphore = new Semaphore(concurrency, concurrency);
            await Task.WhenAll(tests.FilterMask(_cfg.Filter).Select(async test =>
            {
                try
                {
                    semaphore.WaitOne();
                    _logger.Debug($"Run test {test.DisplayName}");

                    await using var scope = _provider.CreateAsyncScope();
                    var target = new Target(scope.ServiceProvider, test, new TestResult());

                    await _executor.ExecuteAsync(target);

                    _logger.Debug($"Complete test {test.DisplayName}");
                    handleResult(target.Test, target.Result);
                }
                finally
                {
                    semaphore.Release();
                }
            }));

            _logger.Debug("Complete tests execution");
        }
    }
}