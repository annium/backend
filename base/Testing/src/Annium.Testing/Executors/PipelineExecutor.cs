using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors
{
    public class PipelineExecutor
    {
        private readonly ITestExecutor[] executors;

        private readonly ILogger<PipelineExecutor> logger;

        public PipelineExecutor(
            IEnumerable<ITestExecutor> executors,
            ILogger<PipelineExecutor> logger
        )
        {
            this.executors = executors.OrderBy(e => e.Order).ToArray();
            this.logger = logger;
        }

        public async Task ExecuteAsync(Target target)
        {
            logger.Trace($"Start pipeline of {target.Test.DisplayName}.");

            var result = target.Result;
            result.ExecutionStart = DateTime.Now;

            foreach (var executor in executors)
            {
                await executor.ExecuteAsync(target);
                if (result.Outcome != TestOutcome.None)
                    break;
            }

            if (result.Outcome == TestOutcome.None)
                result.Outcome = TestOutcome.Passed;

            result.ExecutionEnd = DateTime.Now;

            logger.Trace($"Finished pipeline of {target.Test.DisplayName} with {result.Outcome}.");
        }
    }
}