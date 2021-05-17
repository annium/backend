using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests
{
    public class InitContainerTest
    {
        [Fact]
        public void Managed_NotInitiated_Get_Throws()
        {
            // arrange
            var container = new InitContainer<int>();

            // assert
            ((Action) (() =>
            {
                var _ = container.Value;
            })).Throws<InvalidOperationException>();
        }

        [Fact]
        public void Managed_NotInitiated_Set_Throws()
        {
            // arrange
            var container = new InitContainer<int>();

            // assert
            ((Action) (() => { container.Set(5); })).Throws<InvalidOperationException>();
        }

        [Fact]
        public async Task Managed_Init_ReturnsGetterResult()
        {
            // arrange
            var val = 2;
            var container = new InitContainer<int>(() => val, x => val = x);
            container.SetInit(async () =>
            {
                await Task.Delay(100);
                return 5;
            });

            // act
            var initialByCallback = 0;
            container.OnReady += x => initialByCallback = x;
            var initial = await container;

            // assert
            initial.Is(5);
            initialByCallback.Is(5);
            val.Is(5);
            container.Value.Is(5);
        }

        [Fact]
        public async Task Managed_Set_Works()
        {
            // arrange
            var val = 2;
            var container = new InitContainer<int>(() => val, x => val = x);
            container.SetInit(async () =>
            {
                await Task.Delay(100);
                return 5;
            });

            // act
            await container;
            container.Set(3);

            // assert
            container.Value.Is(3);
            ((int) container).Is(3);
        }

        [Fact]
        public async Task Unmanaged_Init_ReturnsGetterResult()
        {
            // arrange
            var container = new InitContainer<int>();
            container.SetInit(async () =>
            {
                await Task.Delay(100);
                return 5;
            });

            // act
            var initialByCallback = 0;
            container.OnReady += x => initialByCallback = x;
            var initial = await container;

            // assert
            initial.Is(5);
            initialByCallback.Is(5);
            container.Value.Is(5);
        }

        [Fact]
        public async Task Unmanaged_Set_Works()
        {
            // arrange
            var container = new InitContainer<int>();
            container.SetInit(async () =>
            {
                await Task.Delay(100);
                return 5;
            });

            // act
            await container;
            container.Set(3);

            // assert
            container.Value.Is(3);
            ((int) container).Is(3);
        }
    }
}