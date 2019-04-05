using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing.Elements;
using Annium.Testing.Executors;
using Annium.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing
{
    public class TestExecutor
    {
        private readonly IServiceProvider provider;

        private readonly PipelineExecutor executor;
        private readonly ILogger logger;

        public TestExecutor(
            IServiceProvider provider,
            PipelineExecutor executor,
            ILogger logger
        )
        {
            this.provider = provider;
            this.executor = executor;
            this.logger = logger;
        }

        public Task RunTestsAsync(IEnumerable<Test> tests, Action<Test, TestResult> handleResult) =>
            Task.WhenAll(tests.Select(async test =>
            {
                logger.LogDebug($"Run test {test.DisplayName}");

                using(var scope = provider.CreateScope())
                {
                    var target = new Target(scope, test, new TestResult());

                    await executor.ExecuteAsync(target);

                    handleResult(target.Test, target.Result);
                }
            }));
    }
}