using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Testing;
using Xunit;

// ReSharper disable Xunit.XunitTestWithConsoleOutput

namespace Annium.Extensions.Execution.Tests.Background;

public class ConcurrentBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public ConcurrentBackgroundExecutorTests()
        : base(Executor.Background.Concurrent<ConcurrentBackgroundExecutorTests>())
    {
    }

    [Theory]
    [MemberData(nameof(GetRange))]
    public async Task Works(int index)
    {
        Log.SetTestMode();
        // arrange
        Console.WriteLine($"run {index}");
        var parallelism = 4;
        var size = 2;
        var executor = Executor.Background.Concurrent<ConcurrentBackgroundExecutorTests>((uint)parallelism);
        var queue = new ConcurrentQueue<int>();

        // act
        // schedule batch of work
        Parallel.For(0, size, i => executor.Schedule(() =>
        {
            Console.WriteLine($"Enqueue {i}");
            queue.Enqueue(i);
            Helper.SyncLongWork();
            Console.WriteLine($"Enqueue {i + size}");
            queue.Enqueue(i + size);
        }));
        queue.IsEmpty();
        // run executor
        executor.Start();
        // schedule another batch of work
        Parallel.For(2 * size, 3 * size, i => executor.Schedule(() =>
        {
            Console.WriteLine($"Enqueue {i}");
            queue.Enqueue(i);
            Helper.SyncLongWork();
            Console.WriteLine($"Enqueue {i + size}");
            queue.Enqueue(i + size);
        }));

        // assert
        executor.IsAvailable.IsTrue();
        // init disposal
        var disposalTask = executor.DisposeAsync();
        executor.IsAvailable.IsFalse();
        // throws, as not available already
        Wrap.It(() => executor.Schedule(() => { })).Throws<InvalidOperationException>();
        await disposalTask;
        var result = queue.ToArray();
        var sequence = Enumerable.Range(0, 4 * size).ToArray();
        result.Length.Is(sequence.Length);

        /*
         Expected sequence looks like:
            Enqueue 9
            Enqueue 8
            Enqueue 18
            Enqueue 19
            Enqueue 7
            Enqueue 0
            Enqueue 17
            Enqueue 10
            Enqueue 1
            Enqueue 3
         */
        // skipped as is not guaranteed in general
        // take start/end parts for each parallel step
        // for (var i = 0; i < result.Length; i += 2 * parallelism)
        // {
        //     // ensure each start item is followed by end item in next chunk
        //     var nextChunk = result.Skip(i + parallelism).Take(parallelism).ToList();
        //     for (var j = 0; j < parallelism; j++)
        //         nextChunk.Contains(result[i + j] + size).IsTrue();
        // }

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