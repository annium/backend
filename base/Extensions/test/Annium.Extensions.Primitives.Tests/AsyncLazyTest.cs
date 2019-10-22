using System.Threading.Tasks;
using Annium.Testing;

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
        public async Task AsyncFactory_Works()
        {
            // arrange
            var lazy = new AsyncLazy<int>(async () => { await Task.Delay(5); return 10; });

            // act
            var value = await lazy;

            // assert
            value.IsEqual(10);
        }
    }
}