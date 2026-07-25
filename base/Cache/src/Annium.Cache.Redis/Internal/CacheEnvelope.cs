using Annium.Cache.Abstractions;

namespace Annium.Cache.Redis.Internal;

/// <summary>
/// Serialized cache entry stored in Redis: the value plus the expiration metadata needed to enforce
/// logical expiry (via <c>ITimeProvider</c>) independently of Redis' physical TTL.
/// </summary>
/// <typeparam name="TValue">The type of the cached value.</typeparam>
internal sealed record CacheEnvelope<TValue>
{
    /// <summary>
    /// Gets the cached value.
    /// </summary>
    public TValue Value { get; init; } = default!;

    /// <summary>
    /// Gets the expiration mode this entry was created with.
    /// </summary>
    public CacheExpirationMode Mode { get; init; }

    /// <summary>
    /// Gets the absolute expiration deadline as Unix epoch milliseconds (computed from the cache's time provider).
    /// </summary>
    public long ExpiresAtMs { get; init; }

    /// <summary>
    /// Gets the sliding lifetime in milliseconds, or <see langword="null"/> for absolute expiration. Used to
    /// prolong the entry on each access (sliding refresh).
    /// </summary>
    public long? LifetimeMs { get; init; }
}
