using System.Threading.Tasks;
using Testcontainers.Redis;

namespace Annium.Redis.Tests;

/// <summary>
/// Test database setup for Redis integration tests using Testcontainers
/// </summary>
public class Database
{
    /// <summary>
    /// Gets the Redis configuration for test connections
    /// </summary>
    public RedisConfiguration Config { get; } = new();

    /// <summary>
    /// Redis container instance for testing
    /// </summary>
    private readonly RedisContainer _db = new RedisBuilder().Build();

    /// <summary>
    /// Initializes the Redis test database by starting the container and configuring connection
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation</returns>
    public async Task InitAsync()
    {
        await _db.StartAsync();
        Config.Hosts = [new RedisHost(_db.Hostname, _db.GetMappedPublicPort(RedisBuilder.RedisPort))];
    }
}
