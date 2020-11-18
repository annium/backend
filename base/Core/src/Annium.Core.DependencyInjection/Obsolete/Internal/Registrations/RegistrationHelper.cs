using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal static class RegistrationHelper
    {
        public static ServiceDescriptor CreateTypeFactoryDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var factory = CreateTypeFactory(implementationType);

            return new ServiceDescriptor(serviceType, factory, lifetime);
        }

        public static ServiceDescriptor CreateFuncFactoryDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            var factory = CreateFuncFactory(implementationType);

            return new ServiceDescriptor(typeof(Func<>).MakeGenericType(serviceType), factory, lifetime);
        }

        private static Func<IServiceProvider, object> CreateTypeFactory(Type implementationType)
        {
            var (body, sp) = CreateFactoryParts(implementationType);
            var factory = Expression.Lambda(body, sp).Compile();

            return (Func<IServiceProvider, object>) factory;
        }

        private static Func<IServiceProvider, object> CreateFuncFactory(Type implementationType)
        {
            var (body, sp) = CreateFactoryParts(implementationType);
            var factory = Expression.Lambda(Expression.Lambda(body), sp).Compile();

            return (Func<IServiceProvider, object>) factory;
        }

        private static (Expression, ParameterExpression) CreateFactoryParts(Type implementationType)
        {
            var sp = Expression.Parameter(typeof(IServiceProvider));
            var getService = typeof(ServiceProviderServiceExtensions)
                .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider), typeof(Type) })!;
            var typeArg = Expression.Constant(implementationType);
            var body = Expression.Convert(Expression.Call(null, getService, sp, typeArg), implementationType);

            return (body, sp);
        }
    }
}