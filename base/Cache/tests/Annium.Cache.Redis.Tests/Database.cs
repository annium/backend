using System;
using System.Threading.Tasks;
using Annium.Redis;
using Testcontainers.Redis;

namespace Annium.Cache.Redis.Tests;

/// <summary>
/// Test database setup for Redis cache integration tests using Testcontainers. Implements
/// <see cref="IAsyncDisposable"/> so the started container is stopped and removed when the
/// owning test provider is torn down, rather than lingering until the Ryuk reaper at process exit.
/// </summary>
public class Database : IAsyncDisposable
{
    /// <summary>
    /// Gets the Redis connection configuration (consumed by <c>AddRedis</c> / <c>IRedisStorage</c>).
    /// </summary>
    public Annium.Redis.RedisConfiguration Config { get; } = new();

    /// <summary>
    /// Redis container instance for testing.
    /// </summary>
    private readonly RedisContainer _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="Database"/> class with Redis container configuration.
    /// </summary>
    public Database()
    {
        _db = new RedisBuilder("redis:7-alpine").Build();
    }

    /// <summary>
    /// Initializes the Redis test database by starting the container and configuring the connection.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public async Task InitAsync()
    {
        await _db.StartAsync();
        Config.Hosts = [new RedisHost(_db.Hostname, _db.GetMappedPublicPort(RedisBuilder.RedisPort))];
    }

    /// <summary>
    /// Stops and removes the Redis test container.
    /// </summary>
    /// <returns>A value task that completes when the container has been disposed.</returns>
    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _db.DisposeAsync();
    }
}
