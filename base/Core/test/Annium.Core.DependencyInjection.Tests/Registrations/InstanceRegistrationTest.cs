using System;
using Annium.Testing;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests.Registrations
{
    public class InstanceRegistrationTest : TestBase
    {
        [Fact]
        public void AsSelf_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).AsSelf().Singleton();

            // act
            Build();

            // assert
            Get<D>().Is(instance);
        }

        [Fact]
        public void As_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).As(typeof(A)).Singleton();

            // act
            Build();

            // assert
        }

        [Fact]
        public void AsKeyedSelf_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).AsKeyedSelf(nameof(D)).Singleton();

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
            _container.Add(instance).AsKeyed<C, string>(nameof(D)).Singleton();

            // act
            Build();

            // assert
            Get<IIndex<string, C>>()[nameof(D)].Is(instance);
        }

        [Fact]
        public void AsSelfFactory_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).AsSelfFactory().Singleton();

            // act
            Build();

            // assert
            Get<Func<D>>()().Is(instance);
        }

        [Fact]
        public void AsFactory_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).AsFactory<C>().Singleton();

            // act
            Build();

            // assert
            Get<Func<C>>()().Is(instance);
        }

        [Fact]
        public void AsKeyedSelfFactory_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).AsKeyedSelfFactory(nameof(D)).Singleton();

            // act
            Build();

            // assert
            Get<IIndex<string, Func<D>>>()[nameof(D)]().Is(instance);
        }

        [Fact]
        public void AsKeyedFactory_Works()
        {
            // arrange
            var instance = new D(new A());
            _container.Add(instance).AsKeyedFactory<C, string>(nameof(C)).Singleton();

            // act
            Build();

            // assert
            Get<IIndex<string, Func<C>>>()[nameof(C)]().Is(instance);
        }

        private sealed class D : C
        {
            public D(A x) : base(x)
            {
            }
        }

        private class C
        {
            protected C(A _)
            {
            }
        }

        private class A
        {
        }
    }
}