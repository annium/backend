using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Obsolete;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests
{
    public class RegistrationBuilderTest
    {
        private ServiceCollection _services = new ServiceCollection();
        private IServiceProvider _provider = default!;

        [Fact]
        public void As_Works_Ok()
        {
            // act
            _services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .As<A>()
                .SingleInstance();
            Build();

            // assert
            _services.Has(3);
            _services.HasSingleton(typeof(A), typeof(A));
            _services.HasSingletonTypeFactory(typeof(A));
            _services.HasSingleton(typeof(B), typeof(B));
            Get<A>().Is(Get<B>());
        }

        [Fact]
        public void AsSelf_Works_Ok()
        {
            // act
            _services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsSelf()
                .SingleInstance();
            Build();

            // assert
            _services.Has(2);
            _services.HasSingleton(typeof(A), typeof(A));
            _services.HasSingleton(typeof(B), typeof(B));

            Get<A>().As<A>().Is(Get<A>());
            Get<B>().As<B>().Is(Get<B>());
        }

        [Fact]
        public void AsSelfFactory_Works_Ok()
        {
            // act
            _services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsSelfFactory()
                .SingleInstance();
            Build();

            // assert
            _services.Has(4);
            _services.HasSingleton(typeof(A), typeof(A));
            _services.HasSingletonFuncFactory(typeof(A));
            _services.HasSingleton(typeof(B), typeof(B));
            _services.HasSingletonFuncFactory(typeof(B));

            Get<Func<A>>()().As<A>().Is(Get<A>());
            Get<Func<B>>()().As<B>().Is(Get<B>());
        }

        [Fact]
        public void AsImplementedInterfaces_Works_Ok()
        {
            // act
            _services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsImplementedInterfaces()
                .SingleInstance();
            Build();

            // assert
            _services.Has(5);
            _services.HasSingleton(typeof(A), typeof(A));
            _services.HasSingletonTypeFactory(typeof(IA), 2);
            _services.HasSingleton(typeof(B), typeof(B));
            _services.HasSingletonTypeFactory(typeof(IB));

            Get<IA>().Is(Get<B>());
            var arr = Get<IEnumerable<IA>>().ToArray();
            arr.Has(2);
            arr.At(0).Is(Get<A>());
            arr.At(1).Is(Get<B>());
            Get<IB>().Is(Get<B>());
        }

        [Fact]
        public void AsImplementedInterfacesFactories_Works_Ok()
        {
            // act
            _services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsImplementedInterfacesFactories()
                .SingleInstance();
            Build();

            // assert
            _services.Has(5);
            _services.HasSingleton(typeof(A), typeof(A));
            _services.HasSingletonFuncFactory(typeof(IA), 2);
            _services.HasSingleton(typeof(B), typeof(B));
            _services.HasSingletonFuncFactory(typeof(IB));

            Get<Func<IA>>()().Is(Get<B>());
            var arr = Get<IEnumerable<Func<IA>>>().ToArray();
            arr.Has(2);
            arr.At(0)().Is(Get<A>());
            arr.At(1)().Is(Get<B>());
            Get<Func<IB>>()().Is(Get<B>());
        }

        [Fact]
        public void AsAll_Works_Ok()
        {
            // act
            _services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .As<A>()
                .AsSelf()
                .AsSelfFactory()
                .AsImplementedInterfaces()
                .AsImplementedInterfacesFactories()
                .SingleInstance();
            Build();

            // assert
            _services.Has(11);
            _services.HasSingleton(typeof(A), typeof(A));
            _services.HasSingleton(typeof(B), typeof(B));
            _services.HasSingletonTypeFactory(typeof(A));
            _services.HasSingletonTypeFactory(typeof(IA), 2);
            _services.HasSingletonTypeFactory(typeof(IB));
            _services.HasSingletonFuncFactory(typeof(A));
            _services.HasSingletonFuncFactory(typeof(B));
            _services.HasSingletonFuncFactory(typeof(IA), 2);
            _services.HasSingletonFuncFactory(typeof(IB));
        }

        private void Build()
        {
            _provider = _services.BuildServiceProvider();
        }

        private T Get<T>()
            where T : notnull
        {
            return _provider.GetRequiredService<T>();
        }

        internal class A : IA
        {
        }

        internal class B : A, IB
        {
        }

        internal interface IA
        {
        }

        internal interface IB
        {
        }
    }
}