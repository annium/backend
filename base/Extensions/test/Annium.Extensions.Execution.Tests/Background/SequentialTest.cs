using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Execution.Tests.Background;

public class SequentialTest
{
    [Theory]
    [MemberData(nameof(GetRange))]
    // ReSharper disable once xUnit1026
    public async Task SequentialExecutor_NormalFlow_Works(int index)
    {
        Log.SetTestMode();
        Console.WriteLine($"run {index}");

        // arrange
        var executor = Executor.Background.Sequential<SequentialTest>();
        var queue = new ConcurrentQueue<int>();

        // act
        // schedule batch of work
        foreach (var i in Enumerable.Range(0, 20))
            executor.Schedule(async () =>
            {
                await Task.Delay(1);
                queue.Enqueue(i);
            });
        queue.Count.Is(0);
        // run executor
        executor.Start();
        // schedule another batch of work
        foreach (var i in Enumerable.Range(20, 20))
            executor.Schedule(async () =>
            {
                await Task.Delay(1);
                queue.Enqueue(i);
            });

        // assert
        executor.IsAvailable.IsTrue();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => executor.Schedule(() => { })).Throws<InvalidOperationException>();
        await disposalTask;
        queue.Count.Is(40);
        queue.ToArray().IsEqual(Enumerable.Range(0, 40).ToArray());

        Console.WriteLine($"done {index}");
    }

    [Theory]
    [MemberData(nameof(GetRange))]
    // ReSharper disable once xUnit1026
    public async Task SequentialExecutor_Cancellation_Works(int index)
    {
        Log.SetTestMode();
        Console.WriteLine($"run {index}");

        // arrange
        var executor = Executor.Background.Sequential<SequentialTest>();

        // act
        // run executor
        executor.Start();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        await disposalTask;

        Console.WriteLine($"done {index}");
    }

    public static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 20).Select(x => new object[] { x });
}