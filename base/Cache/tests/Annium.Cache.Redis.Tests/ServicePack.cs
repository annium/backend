using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Packs;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Service pack for configuring Redis cache dependencies for testing.
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers the Redis cache services required for testing.
    /// </summary>
    /// <param name="container">The service container to register services with.</param>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRedisCache(ServiceLifetime.Singleton);
    }
}
