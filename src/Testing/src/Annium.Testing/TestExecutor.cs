using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing.Elements;
using Annium.Testing.Executors;
using Annium.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing
{
    public class TestExecutor
    {
        private readonly Configuration cfg;

        private readonly IServiceProvider provider;

        private readonly PipelineExecutor executor;

        private readonly ILogger logger;

        public TestExecutor(
            Configuration cfg,
            IServiceProvider provider,
            PipelineExecutor executor,
            ILogger logger
        )
        {
            this.cfg = cfg;
            this.provider = provider;
            this.executor = executor;
            this.logger = logger;
        }

        public async Task RunTestsAsync(IEnumerable<Test> tests, Action<Test, TestResult> handleResult)
        {
            logger.LogDebug("Start tests execution");

            var concurrency = Environment.ProcessorCount;

            using(var semaphore = new Semaphore(concurrency, concurrency))
            {
                await Task.WhenAll(tests.FilterMask(cfg.Filter).Select(async test =>
                {
                    try
                    {
                        semaphore.WaitOne();
                        logger.LogDebug($"Run test {test.DisplayName}");

                        using(var scope = provider.CreateScope())
                        {
                            var target = new Target(scope, test, new TestResult());

                            await executor.ExecuteAsync(target);

                            logger.LogDebug($"Complete test {test.DisplayName}");

                            handleResult(target.Test, target.Result);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            logger.LogDebug("Complete tests execution");
        }
    }
}