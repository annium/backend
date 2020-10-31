using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Core.DependencyInjection.Tests
{
    public class RegistrationBuilderTest
    {
        [Fact]
        public void As_Works_Ok()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .As<A>()
                .SingleInstance();

            // assert
            services.Has(3);
            services.HasSingleton(typeof(A), typeof(A));
            services.HasSingleton(typeof(A), typeof(B));
            services.HasSingleton(typeof(B), typeof(B));
            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<A>().As<B>();
        }

        [Fact]
        public void AsSelf_Works_Ok()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsSelf()
                .SingleInstance();

            // assert
            services.Has(2);
            services.HasSingleton(typeof(A), typeof(A));
            services.HasSingleton(typeof(B), typeof(B));
            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<A>().As<A>();
            provider.GetRequiredService<B>().As<B>();
        }

        [Fact]
        public void AsSelfFactory_Works_Ok()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsSelfFactory()
                .SingleInstance();

            // assert
            services.Has(4);
            services.HasSingleton(typeof(A), typeof(A));
            services.HasSingletonFactory(typeof(A));
            services.HasSingleton(typeof(B), typeof(B));
            services.HasSingletonFactory(typeof(B));
            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<Func<A>>()().As<A>();
            provider.GetRequiredService<Func<B>>()().As<B>();
        }

        [Fact]
        public void AsImplementedInterfaces_Works_Ok()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsImplementedInterfaces()
                .SingleInstance();

            // assert
            services.Has(5);
            services.HasSingleton(typeof(A), typeof(A));
            services.HasSingleton(typeof(IA), typeof(A));
            services.HasSingleton(typeof(B), typeof(B));
            services.HasSingleton(typeof(IA), typeof(B));
            services.HasSingleton(typeof(IB), typeof(B));
            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IA>().As<B>();
            var arr = provider.GetRequiredService<IEnumerable<IA>>().ToArray();
            arr.Has(2);
            arr.At(0).GetType().Is(typeof(A));
            arr.At(1).GetType().Is(typeof(B));
            provider.GetRequiredService<IB>().As<B>();
        }

        [Fact]
        public void AsImplementedInterfacesFactories_Works_Ok()
        {
            // arrange
            var services = new ServiceCollection();

            // act
            services
                .AddAssemblyTypes(GetType().Assembly)
                .AssignableTo<IA>()
                .AsImplementedInterfacesFactories()
                .SingleInstance();

            // assert
            services.Has(5);
            services.HasSingleton(typeof(A), typeof(A));
            services.HasSingletonFactory(typeof(IA), 2);
            services.HasSingleton(typeof(B), typeof(B));
            services.HasSingletonFactory(typeof(IB));
            var provider = services.BuildServiceProvider();
            var a = provider.GetRequiredService<Func<IA>>()();
            a.GetType().Is(typeof(B));
            var arr = provider.GetRequiredService<IEnumerable<Func<IA>>>().ToArray();
            arr.Has(2);
            arr.At(0)().GetType().Is(typeof(A));
            arr.At(1)().GetType().Is(typeof(B));
            var b = provider.GetRequiredService<Func<IB>>()();
            b.GetType().Is(typeof(B));
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