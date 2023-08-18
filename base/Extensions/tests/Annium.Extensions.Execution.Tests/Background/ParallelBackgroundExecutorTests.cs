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

public class ParallelBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public ParallelBackgroundExecutorTests()
        : base(Executor.Background.Parallel<ParallelBackgroundExecutorTests>())
    {
    }

    [Theory]
    [MemberData(nameof(GetRange))]
    public async Task Works(int index)
    {
        Log.SetTestMode();
        // arrange
        Console.WriteLine($"run {index}");
        var executor = Executor.Background.Parallel<ParallelBackgroundExecutorTests>();
        var queue = new ConcurrentQueue<int>();

        // act
        // schedule batch of work
        Parallel.For(0, 10, i => executor.Schedule(async () =>
        {
            queue.Enqueue(i);
            await Helper.AsyncLongWork();
            queue.Enqueue(i + 10);
        }));
        queue.IsEmpty();
        // run executor
        executor.Start();
        // schedule another batch of work
        Parallel.For(20, 30, i => executor.Schedule(async () =>
        {
            queue.Enqueue(i);
            await Helper.AsyncLongWork();
            queue.Enqueue(i + 10);
        }));

        // assert
        executor.IsAvailable.IsTrue();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => executor.Schedule(() => { })).Throws<InvalidOperationException>();
        await disposalTask;
        var sequence = Enumerable.Range(0, 10).SelectMany(x => new[] { x, x + 10 })
            .Concat(Enumerable.Range(20, 10).SelectMany(x => new[] { x, x + 10 }))
            .ToArray();
        var result = queue.ToArray();
        result.IsNotEqual(sequence);
        result.Length.Is(sequence.Length);
        foreach (var num in sequence)
            result.Contains(num).IsTrue();

        Console.WriteLine("done");
    }

    public static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 10).Select(x => new object[] { x });

    [Fact]
    public async Task HandlesFailure()
    {
        await HandlesFailure_Base();
    }

    [Fact]
    public async Task Schedule_SyncAction()
    {
        await Schedule_SyncAction_Base();
    }

    [Fact]
    public async Task Schedule_SyncCancellableAction()
    {
        await Schedule_SyncCancellableAction_Base();
    }

    [Fact]
    public async Task Schedule_AsyncAction()
    {
        await Schedule_AsyncAction_Base();
    }

    [Fact]
    public async Task Schedule_AsyncCancellableAction()
    {
        await Schedule_AsyncCancellableAction_Base();
    }
}