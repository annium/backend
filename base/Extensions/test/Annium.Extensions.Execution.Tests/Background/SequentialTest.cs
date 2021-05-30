using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Execution.Tests.Background
{
    public class SequentialTest
    {
        [Fact]
        public async Task SequentialExecutor_Works()
        {
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
            ((Action) (() => executor.Schedule(() => { }))).Throws<InvalidOperationException>();
            await disposalTask;
            queue.Count.Is(40);
            queue.ToArray().IsEqual(Enumerable.Range(0, 40).ToArray());
        }
    }
}