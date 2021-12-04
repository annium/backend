using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

public class SingleRegistrationTest : TestBase
{
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsSelf().Singleton();

        // act
        Build();

        // assert
        _container.HasSingleton(typeof(B), typeof(B));
        Get<B>().AsExact<B>();
    }

    [Fact]
    public void As_Works()
    {
        // arrange
        _container.Add(typeof(B)).As(typeof(A)).Singleton();

        // act
        Build();

        // assert
        _container.HasSingletonTypeFactory(typeof(A));
        Get<A>().Is(Get<B>());
    }

    [Fact]
    public void AsInterfaces_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsInterfaces().Singleton();

        // act
        Build();

        // assert
        Get<IA>().Is(Get<IB>());
        Get<IB>().Is(Get<IB>());
    }

    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsKeyedSelf(nameof(B)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, B>>()[nameof(B)].Is(Get<B>());
    }

    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsKeyed(typeof(A), nameof(B)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, A>>()[nameof(B)].Is(Get<B>());
    }

    [Fact]
    public void AsSelfFactory_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsSelfFactory().Singleton();

        // act
        Build();

        // assert
        Get<Func<B>>()().Is(Get<B>());
    }

    [Fact]
    public void AsFactory_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsFactory<A>().Singleton();

        // act
        Build();

        // assert
        Get<Func<A>>()().Is(Get<B>());
    }

    [Fact]
    public void AsKeyedSelfFactory_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsKeyedSelfFactory(nameof(B)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, Func<B>>>()[nameof(B)]().Is(Get<B>());
    }

    [Fact]
    public void AsKeyedFactory_Works()
    {
        // arrange
        _container.Add(typeof(B)).AsKeyedFactory(typeof(A), nameof(B)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, Func<A>>>()[nameof(B)]().Is(Get<B>());
    }

    private sealed class B : A, IB
    {
    }

    private class A : IA
    {
    }

    private interface IB : IA
    {
    }

    private interface IA
    {
    }
}