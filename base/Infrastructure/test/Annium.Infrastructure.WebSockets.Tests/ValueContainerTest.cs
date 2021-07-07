using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Testing;
using Xunit;

namespace Annium.Infrastructure.WebSockets.Tests
{
    public class ValueContainerTest
    {
        [Fact]
        public void NotInitiated_Get_Throws()
        {
            // arrange
            var state = GetState();

            // assert
            ((Action) (() =>
            {
                var _ = state.A.Value;
            })).Throws<InvalidOperationException>();
        }

        [Fact]
        public void NotInitiated_Set_Throws()
        {
            // arrange
            var state = GetState();

            // assert
            ((Action) (() => { state.A.Set(5); })).Throws<InvalidOperationException>();
        }

        [Fact]
        public async Task Init_ReturnsLoaderResult()
        {
            // arrange
            var state = GetState();

            // act
            var initialByCallback = 0m;
            state.B.OnChange += x => initialByCallback = x;
            var initial = await state.B;

            // assert
            initial.GetType().Is(typeof(decimal));
            initialByCallback.Is(initial);
            state.B.Value.Is(initial);
            state.B.Value.Is(state.A.Value * 2);
        }

        [Fact]
        public async Task AwaitAfterSet_ReturnsValidResult()
        {
            // arrange
            var state = GetState();

            // act
            var initialByCallback = 0;
            state.A.OnChange += x => initialByCallback = x;
            var initial = await state.A;
            initialByCallback.Is(initial);
            var second = 7;
            state.A.Set(second);
            var reacquired = await state.A;

            // assert
            initialByCallback.Is(second);
            reacquired.Is(second);
            state.A.Value.Is(second);
        }

        [Fact]
        public async Task Set_Works()
        {
            // arrange
            var value = 8m;
            var state = GetState();

            // act
            var valueByCallback = 0m;
            state.B.OnChange += x => valueByCallback = x;
            await state.B;
            state.B.Set(value);

            // assert
            state.B.Value.Is(value);
            valueByCallback.Is(value);
        }

        private ConnectionState GetState()
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddWebSocketServer<ConnectionState>((_, _) => { });

            var sp = container.BuildServiceProvider();
            var factory = sp.Resolve<Func<Guid, ConnectionState>>();

            return factory(Guid.NewGuid());
        }

        private class IntValueLoader : IValueLoader<ConnectionState, int>
        {
            public async ValueTask<int> LoadAsync(ConnectionState _)
            {
                var rnd = new Random();
                await Task.Delay(10);
                return rnd.Next();
            }
        }

        private class DecimalValueLoader : IValueLoader<ConnectionState, decimal>
        {
            public async ValueTask<decimal> LoadAsync(ConnectionState state)
            {
                await Task.Delay(10);
                var intValue = await state.A;
                return intValue * 2;
            }
        }

        private class ConnectionState : ConnectionStateBase
        {
            public IValueContainer<ConnectionState, int> A { get; }
            public IValueContainer<ConnectionState, decimal> B { get; }

            public ConnectionState(
                IValueContainer<ConnectionState, int> a,
                IValueContainer<ConnectionState, decimal> b
            )
            {
                A = a;
                B = b;
            }
        }
    }
}