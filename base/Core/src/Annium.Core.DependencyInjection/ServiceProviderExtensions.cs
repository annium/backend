using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static T Resolve<T>(this IServiceProvider provider)
            where T : notnull
            => provider.GetRequiredService<T>();

        public static object Resolve(this IServiceProvider provider, Type type)
            => provider.GetRequiredService(type);

        public static T ResolveKeyed<TKey, T>(this IServiceProvider provider, TKey key)
            where TKey : notnull
            where T : notnull
            => provider.GetRequiredService<IIndex<TKey, T>>()[key];

        public static IServiceScope CreateScope(this IServiceProvider provider)
            => ServiceProviderServiceExtensions.CreateScope(provider);
    }
}