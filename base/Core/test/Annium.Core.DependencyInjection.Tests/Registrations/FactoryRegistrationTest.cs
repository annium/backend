using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations;

public class FactoryRegistrationTest : TestBase
{
    [Fact]
    public void AsSelf_Works()
    {
        // arrange
        var a = new A();
        _container.Add(_ => new D(a)).AsSelf().Singleton();

        // act
        Build();

        // assert
        _container.HasSingletonTypeFactory(typeof(D));
        var d = Get<D>();
        d.A.Is(a);
        Get<D>().Is(d);
    }

    [Fact]
    public void As_Works()
    {
        // arrange
        var instance = new D(new A());
        _container.Add(_ => instance).As<C>().Singleton();

        // act
        Build();

        // assert
        _container.HasSingletonTypeFactory(typeof(C));
        Get<C>().Is(instance);
    }

    [Fact]
    public void AsInterfaces_Works()
    {
        // arrange
        var instance = new D(new A());
        _container.Add(_ => instance).AsInterfaces().Singleton();

        // act
        Build();

        // assert
        _container.HasSingletonTypeFactory(typeof(IX));
        Get<IX>().Is(instance);
    }

    [Fact]
    public void AsKeyedSelf_Works()
    {
        // arrange
        var instance = new D(new A());
        _container.Add(_ => instance).AsKeyedSelf(nameof(D)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, D>>()[nameof(D)].Is(instance);
    }

    [Fact]
    public void AsKeyed_Works()
    {
        // arrange
        var instance = new D(new A());
        _container.Add(_ => instance).AsKeyed<C, string>(nameof(C)).Singleton();

        // act
        Build();

        // assert
        Get<IIndex<string, C>>()[nameof(C)].Is(instance);
    }

    private sealed class D : C, IX
    {
        public D(A x) : base(x)
        {
        }
    }

    private class C
    {
        public A A { get; }

        protected C(A a)
        {
            A = a;
        }
    }

    private class A
    {
    }

    private interface IX
    {
    }
}