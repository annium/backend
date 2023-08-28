using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors;

public class PipelineExecutor : ILogSubject
{
    public ILogger Logger { get; }
    private readonly ITestExecutor[] _executors;

    public PipelineExecutor(
        IEnumerable<ITestExecutor> executors,
        ILogger logger
    )
    {
        _executors = executors.OrderBy(e => e.Order).ToArray();
        Logger = logger;
    }

    public async Task ExecuteAsync(Target target)
    {
        this.Log().Trace($"Start pipeline of {target.Test.DisplayName}.");

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

        this.Log().Trace($"Finished pipeline of {target.Test.DisplayName} with {result.Outcome}.");
    }
}