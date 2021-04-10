using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Execution.Tests.Background
{
    public class ParallelTest
    {
        [Fact]
        public async Task ParallelExecutor_Works()
        {
            // arrange
            var executor = Executor.Background.Parallel();
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
            executor.Start(CancellationToken.None);
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
            ((Action) (() => executor.Schedule(() => { }))).Throws<InvalidOperationException>();
            await disposalTask;
            counter.Is(200);
        }
    }
}