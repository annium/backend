using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal static class RegistrationHelper
    {
        public static Func<IServiceProvider, object> CreateFactory(Type implementationType)
        {
            var sp = Expression.Parameter(typeof(IServiceProvider));
            var getService = typeof(ServiceProviderServiceExtensions)
                .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), new[] { typeof(IServiceProvider), typeof(Type) })!;
            var typeArg = Expression.Constant(implementationType);
            var body = Expression.Convert(Expression.Call(null, getService, sp, typeArg), implementationType);
            var factory = Expression.Lambda(Expression.Lambda(body), sp).Compile();

            return (Func<IServiceProvider, object>) factory;
        }
    }
}