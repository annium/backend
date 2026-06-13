using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;

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
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddRedisCache(ServiceLifetime.Singleton);
        return Task.CompletedTask;
    }
}
