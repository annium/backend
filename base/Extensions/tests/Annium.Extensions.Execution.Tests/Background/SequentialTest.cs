using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Testing;
using Xunit;

// ReSharper disable Xunit.XunitTestWithConsoleOutput

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
        foreach (var i in Enumerable.Range(0, 10))
            executor.Schedule(async () =>
            {
                queue.Enqueue(i);
                await Helper.AsyncFastWork();
                queue.Enqueue(i + 10);
            });
        queue.IsEmpty();
        // run executor
        executor.Start();
        // schedule another batch of work
        foreach (var i in Enumerable.Range(20, 10))
            executor.Schedule(async () =>
            {
                queue.Enqueue(i);
                await Helper.AsyncFastWork();
                queue.Enqueue(i + 10);
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
        var sequence = Enumerable.Range(0, 10).SelectMany(x => new[] { x, x + 10 })
            .Concat(Enumerable.Range(20, 10).SelectMany(x => new[] { x, x + 10 }))
            .ToArray();
        queue.ToArray().IsEqual(sequence);

        Console.WriteLine($"done {index}");
    }

    public static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });

    [Fact]
    // ReSharper disable once xUnit1026
    public async Task SequentialExecutor_Cancellation_Works()
    {
        Log.SetTestMode();

        // arrange
        var executor = Executor.Background.Sequential<SequentialTest>();

        // act
        // run executor
        executor.Start();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        await disposalTask;
    }
}