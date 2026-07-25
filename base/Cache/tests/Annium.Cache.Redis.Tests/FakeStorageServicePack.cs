using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Redis;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Service pack wiring the Redis cache over an in-process <see cref="GatedRedisStorage"/> instead of a real
/// Testcontainers backend, so storage-timing races (sliding-refresh vs remove, post-write invalidation) can be
/// interleaved deterministically.
/// </summary>
public class FakeStorageServicePack : ServicePackBase
{
    /// <summary>
    /// Registers a JSON serializer, the gated in-memory storage (as <see cref="IRedisStorage"/>), and the cache.
    /// </summary>
    /// <param name="container">The service container to register services with.</param>
    /// <param name="provider">The service provider for resolving dependencies.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddSerializers().WithJson(isDefault: true);
        container.Add<IRedisStorage, GatedRedisStorage>().Singleton();
        container.AddRedisCache(cfg => cfg.KeyPrefix = "test:");
        return Task.CompletedTask;
    }
}
