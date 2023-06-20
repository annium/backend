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

public class ConcurrentTest
{
    [Theory]
    [MemberData(nameof(GetRange))]
    public async Task ConcurrentExecutor_Works(int index)
    {
        Log.SetTestMode();
        // arrange
        Console.WriteLine($"run {index}");
        var parallelism = 2;
        var size = parallelism * 5;
        var executor = Executor.Background.Concurrent<ConcurrentTest>(2);
        var queue = new ConcurrentQueue<int>();

        // act
        // schedule batch of work
        Parallel.For(0, size, i => executor.Schedule(async () =>
        {
            Console.WriteLine($"Enqueue {i}");
            queue.Enqueue(i);
            await Helper.AsyncLongWork();
            Console.WriteLine($"Enqueue {i + size}");
            queue.Enqueue(i + size);
        }));
        queue.IsEmpty();
        // run executor
        executor.Start();
        // schedule another batch of work
        Parallel.For(2 * size, 3 * size, i => executor.Schedule(async () =>
        {
            Console.WriteLine($"Enqueue {i}");
            queue.Enqueue(i);
            await Helper.AsyncLongWork();
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

    public static IEnumerable<object[]> GetRange() => Enumerable.Range(0, 1).Select(x => new object[] { x });

    [Fact]
    public async Task ConcurrentExecutor_CompletesOnFailure()
    {
        Log.SetTestMode();
        // arrange
        var executor = Executor.Background.Concurrent<ConcurrentTest>(10);
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