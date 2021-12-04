using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests;

public class ServiceContainerTest : TestBase
{
    [Fact]
    public void Add_WritesToCollectionImmediately()
    {
        // arrange
        var instance = new A();

        // act
        _container.Add(instance).AsSelf().Singleton();
        Build();

        // assert
        Get<A>().Is(instance);
    }

    [Fact]
    public void ContainsType_Works()
    {
        // arrange
        _container.Add<A>().AsSelf().Singleton();

        // assert
        _container.Contains(ServiceDescriptor.Type(typeof(A), typeof(A), ServiceLifetime.Singleton)).IsTrue();
    }

    [Fact]
    public void ContainsFactory_Works()
    {
        // arrange
        static B factory(IServiceProvider _) => new();
        _container.Add(factory).AsSelf().Singleton();

        // assert
        _container.Contains(ServiceDescriptor.Factory(typeof(B), factory, ServiceLifetime.Singleton)).IsTrue();
    }

    [Fact]
    public void ContainsInstance_Works()
    {
        // arrange
        var instance = new B();
        _container.Add(instance).AsSelf().Singleton();

        // assert
        _container.Contains(ServiceDescriptor.Instance(typeof(B), instance, ServiceLifetime.Singleton)).IsTrue();
    }

    private sealed record B : A;

    private record A;
}