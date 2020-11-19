using System;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests
{
    public class ServiceContainerTest
    {
        private readonly ServiceContainer _container = new ServiceContainer();
        private IServiceProvider _provider = default!;

        [Fact]
        public void Add_Works_Ok()
        {
            // arrange
            var instance = new object();

            // act
            _container.Add(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton(instance));
            Build();

            // assert
            _container.Has(1);
            Get<object>().Is(instance);
        }

        private void Build()
        {
            _provider = _container.BuildServiceProvider();
        }

        private T Get<T>()
            where T : notnull
        {
            return _provider.GetRequiredService<T>();
        }
    }
}