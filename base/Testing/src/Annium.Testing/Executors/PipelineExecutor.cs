using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing.Elements;
using Annium.Testing.Logging;

namespace Annium.Testing.Executors
{
    public class PipelineExecutor
    {
        private readonly ITestExecutor[] executors;

        private readonly ILogger logger;

        public PipelineExecutor(
            IEnumerable<ITestExecutor> executors,
            ILogger logger
        )
        {
            this.executors = executors.OrderBy(e => e.Order).ToArray();
            this.logger = logger;
        }

        public async Task ExecuteAsync(Target target)
        {
            logger.LogTrace($"Start pipeline of {target.Test.DisplayName}.");

            var result = target.Result;
            result.ExecutionStart = DateTime.Now;

            foreach (var executor in this.executors)
            {
                await executor.ExecuteAsync(target);
                if (result.Outcome != TestOutcome.None)
                    break;
            }

            if (result.Outcome == TestOutcome.None)
                result.Outcome = TestOutcome.Passed;

            result.ExecutionEnd = DateTime.Now;

            logger.LogTrace($"Finished pipeline of {target.Test.DisplayName} with {result.Outcome}.");
        }
    }
}