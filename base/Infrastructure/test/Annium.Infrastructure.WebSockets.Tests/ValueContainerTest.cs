using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Domain.Models;
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
            var container = GetServiceProvider().Resolve<IValueContainer<object>>();

            // assert
            ((Action) (() =>
            {
                var _ = container.Value;
            })).Throws<InvalidOperationException>();
        }

        [Fact]
        public void NotInitiated_Set_Throws()
        {
            // arrange
            var container = GetServiceProvider().Resolve<IValueContainer<object>>();

            // assert
            ((Action) (() => { container.Set(new object()); })).Throws<InvalidOperationException>();
        }

        [Fact]
        public async Task Init_ReturnsLoaderResult()
        {
            // arrange
            var container = GetServiceProvider().Resolve<IValueContainer<object>>();

            // act
            var initialByCallback = new object();
            container.OnChange += x => initialByCallback = x;
            var initial = await container;

            // assert
            initial.GetType().Is(typeof(object));
            initialByCallback.Is(initial);
            container.Value.Is(initial);
        }

        [Fact]
        public async Task AwaitAfterSet_ReturnsValidResult()
        {
            // arrange
            var container = GetServiceProvider().Resolve<IValueContainer<object>>();

            // act
            var initialByCallback = new object();
            container.OnChange += x => initialByCallback = x;
            var initial = await container;
            initialByCallback.Is(initial);
            var second = new object();
            container.Set(second);
            var reacquired = await container;

            // assert
            initialByCallback.Is(second);
            reacquired.Is(second);
            container.Value.Is(second);
        }

        [Fact]
        public async Task Set_Works()
        {
            // arrange
            var value = new object();
            var container = GetServiceProvider().Resolve<IValueContainer<object>>();

            // act
            var valueByCallback = new object();
            container.OnChange += x => valueByCallback = x;
            await container;
            container.Set(value);

            // assert
            container.Value.Is(value);
            valueByCallback.Is(value);
        }

        private IServiceProvider GetServiceProvider()
        {
            var container = new ServiceContainer();
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddWebSocketServer<ConnectionState>((_, _) => { });

            return container.BuildServiceProvider();
        }

        private class ObjectValueLoader : IValueLoader<object>
        {
            public async ValueTask<object> LoadAsync()
            {
                await Task.Delay(10);
                return new();
            }
        }

        private class ConnectionState : ConnectionStateBase
        {
        }
    }
}