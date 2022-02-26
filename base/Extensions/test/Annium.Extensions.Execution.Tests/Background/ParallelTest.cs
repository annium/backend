using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Execution.Tests.Background;

public class ParallelTest
{
    [Theory]
    [MemberData(nameof(GetRange))]
    // ReSharper disable once xUnit1026
    public async Task ParallelExecutor_Works(int index)
    {
        Log.SetTestMode();
        // arrange
        Console.WriteLine($"run {index}");
        var executor = Executor.Background.Parallel<ParallelTest>();
        var counter = 0;

        // act
        // schedule batch of work
        Parallel.For(0, 100, _ => executor.Schedule(async () =>
        {
            await Task.Delay(10);
            Interlocked.Increment(ref counter);
        }));
        counter.Is(0);
        // run executor
        executor.Start();
        // schedule another batch of work
        Parallel.For(0, 100, _ => executor.Schedule(async () =>
        {
            await Task.Delay(10);
            Interlocked.Increment(ref counter);
        }));

        // assert
        executor.IsAvailable.IsTrue();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => executor.Schedule(() => { })).Throws<InvalidOperationException>();
        await disposalTask;
        counter.Is(200);

        Console.WriteLine($"done");
    }

    private static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 20).Select(x => new object[] { x });

    [Fact]
    public async Task ParallelExecutor_CompletesOnFailure()
    {
        Log.SetTestMode();
        // arrange
        var executor = Executor.Background.Parallel<ParallelTest>();
        var successes = 0;
        var failures = 0;

        // act
        // schedule batch of work
        Parallel.For(1, 100, i => executor.Schedule(async () =>
        {
            await Task.Delay(10);
            if (i % 90 == 0)
            {
                Interlocked.Increment(ref failures);
                throw new Exception("Some parallel failure");
            }

            Interlocked.Increment(ref successes);
        }));
        successes.Is(0);
        failures.Is(0);
        // run executor
        executor.Start();
        // schedule another batch of work
        Parallel.For(1, 100, i => executor.Schedule(async () =>
        {
            await Task.Delay(10);
            if (i % 90 == 0)
            {
                Interlocked.Increment(ref failures);
                throw new Exception("Some parallel failure");
            }

            Interlocked.Increment(ref successes);
        }));

        // assert
        executor.IsAvailable.IsTrue();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => executor.Schedule(() => { })).Throws<InvalidOperationException>();
        await disposalTask;
        successes.Is(196);
        failures.Is(2);
    }
}