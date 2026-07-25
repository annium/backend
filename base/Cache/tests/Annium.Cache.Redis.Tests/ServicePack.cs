using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Redis;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Service pack for configuring Redis cache dependencies for testing: a JSON serializer, a Testcontainers
/// Redis backend, the shared <c>IRedisStorage</c> (via <c>AddRedis</c>), and the cache itself.
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
        container.AddSerializers().WithJson(isDefault: true);
        container.Add<Database>().AsSelf().Singleton();
        container.Add(sp => sp.Resolve<Database>().Config).AsSelf().Singleton();
        container.AddRedis();
        container.AddRedisCache(cfg => cfg.KeyPrefix = "test:");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets up the test environment by initializing the Redis database container.
    /// </summary>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous setup.</returns>
    public override async Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        await provider.Resolve<Database>().InitAsync();
    }
}
