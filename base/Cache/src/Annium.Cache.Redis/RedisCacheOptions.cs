namespace Annium.Cache.Redis;

/// <summary>
/// Options for the Redis-backed cache. Connection settings are owned by <c>Annium.Redis</c>
/// (registered via <c>AddRedis</c>); these options cover cache-level concerns only.
/// </summary>
public record RedisCacheOptions
{
    /// <summary>
    /// Gets or sets the prefix prepended to every cache key, namespacing entries so that the cache
    /// can share a Redis instance with other <c>IRedisStorage</c> consumers without key collisions.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;
}
