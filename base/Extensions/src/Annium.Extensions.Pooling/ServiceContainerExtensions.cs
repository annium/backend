using System;
using System.Threading.Tasks;
using Annium.Extensions.Pooling;
using Annium.Logging.Abstractions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddObjectCache<TKey, TValue>(
            this IServiceContainer container,
            Func<IServiceProvider, (Func<TKey, Task<TValue>> factory, Func<TValue, Task> suspend, Func<TValue, Task> resume)> factory,
            ServiceLifetime lifetime
        )
            where TKey : notnull
            where TValue : notnull
        {
            container.Add<IObjectCache<TKey, TValue>>(sp =>
            {
                var logger = sp.Resolve<ILogger<ObjectCache<TKey, TValue>>>();
                var cfg = factory(sp);

                return new ObjectCache<TKey, TValue>(cfg.factory, cfg.suspend, cfg.resume, logger);
            }).AsSelf().In(lifetime);

            return container;
        }

        public static IServiceContainer AddObjectCache<TKey, TValue>(
            this IServiceContainer container,
            Func<IServiceProvider, Func<TKey, Task<TValue>>> factory,
            ServiceLifetime lifetime
        )
            where TKey : notnull
            where TValue : notnull
        {
            container.Add<IObjectCache<TKey, TValue>>(sp =>
            {
                var logger = sp.Resolve<ILogger<ObjectCache<TKey, TValue>>>();

                return new ObjectCache<TKey, TValue>(factory(sp), logger);
            }).AsSelf().In(lifetime);

            return container;
        }

        public static IServiceContainer AddObjectCache<TKey, TValue>(
            this IServiceContainer container,
            Func<IServiceProvider, (Func<TKey, Task<ICacheReference<TValue>>> factory, Func<TValue, Task> suspend, Func<TValue, Task> resume)> factory,
            ServiceLifetime lifetime
        )
            where TKey : notnull
            where TValue : notnull
        {
            container.Add<IObjectCache<TKey, TValue>>(sp =>
            {
                var logger = sp.Resolve<ILogger<ObjectCache<TKey, TValue>>>();
                var cfg = factory(sp);

                return new ObjectCache<TKey, TValue>(cfg.factory, cfg.suspend, cfg.resume, logger);
            }).AsSelf().In(lifetime);

            return container;
        }

        public static IServiceContainer AddObjectCache<TKey, TValue>(
            this IServiceContainer container,
            Func<IServiceProvider, Func<TKey, Task<ICacheReference<TValue>>>> factory,
            ServiceLifetime lifetime
        )
            where TKey : notnull
            where TValue : notnull
        {
            container.Add<IObjectCache<TKey, TValue>>(sp =>
            {
                var logger = sp.Resolve<ILogger<ObjectCache<TKey, TValue>>>();
                return new ObjectCache<TKey, TValue>(factory(sp), logger);
            }).AsSelf().In(lifetime);

            return container;
        }
    }
}