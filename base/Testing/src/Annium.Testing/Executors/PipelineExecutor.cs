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
        private readonly ITestExecutor[] _executors;

        private readonly ILogger<PipelineExecutor> _logger;

        public PipelineExecutor(
            IEnumerable<ITestExecutor> executors,
            ILogger<PipelineExecutor> logger
        )
        {
            _executors = executors.OrderBy(e => e.Order).ToArray();
            _logger = logger;
        }

        public async Task ExecuteAsync(Target target)
        {
            _logger.Trace($"Start pipeline of {target.Test.DisplayName}.");

            var result = target.Result;
            result.ExecutionStart = DateTime.Now;

            foreach (var executor in _executors)
            {
                await executor.ExecuteAsync(target);
                if (result.Outcome != TestOutcome.None)
                    break;
            }

            if (result.Outcome == TestOutcome.None)
                result.Outcome = TestOutcome.Passed;

            result.ExecutionEnd = DateTime.Now;

            _logger.Trace($"Finished pipeline of {target.Test.DisplayName} with {result.Outcome}.");
        }
    }
}