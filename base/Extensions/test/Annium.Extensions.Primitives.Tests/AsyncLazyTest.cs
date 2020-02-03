using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Primitives.Tests
{
    public class AsyncLazyTest
    {
        [Fact]
        public async Task SyncFactory_Works()
        {
            // arrange
            var lazy = new AsyncLazy<int>(() => 10);

            // act
            var value = await lazy;

            // assert
            value.IsEqual(10);
        }

        [Fact]
        public async Task SyncFactory_Concurrent_Works()
        {
            // arrange
            var counter = 0;
            var lazy = new AsyncLazy<int>(() => Interlocked.Increment(ref counter));

            // act
            var values = await Task.WhenAll(
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy)
            );

            // assert
            values.IsEqual(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            counter.IsEqual(1);
        }

        [Fact]
        public async Task AsyncFactory_Works()
        {
            // arrange
            var lazy = new AsyncLazy<int>(async () =>
            {
                await Task.Delay(5);
                return 10;
            });

            // act
            var value = await lazy;

            // assert
            value.IsEqual(10);
        }

        [Fact]
        public async Task AsyncFactory_Concurrent_Works()
        {
            // arrange
            var counter = 0;
            var lazy = new AsyncLazy<int>(async () =>
            {
                await Task.Delay(5);
                return Interlocked.Increment(ref counter);
            });

            // act
            var values = await Task.WhenAll(
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy),
                Task.Run(async () => await lazy)
            );

            // assert
            values.IsEqual(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            counter.IsEqual(1);
        }
    }
}