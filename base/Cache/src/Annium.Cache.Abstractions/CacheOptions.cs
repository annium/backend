using NodaTime;

namespace Annium.Cache.Abstractions;

/// <summary>
/// Configuration options for cache item expiration
/// </summary>
public sealed record CacheOptions
{
    /// <summary>
    /// Creates cache options with absolute expiration at the specified moment
    /// </summary>
    /// <param name="moment">The absolute moment when the cache item should expire</param>
    /// <returns>Cache options configured for absolute expiration</returns>
    public static CacheOptions WithAbsoluteExpiration(Instant moment)
    {
        return new CacheOptions(moment, Duration.Zero);
    }

    /// <summary>
    /// Creates cache options with sliding expiration using the specified lifetime
    /// </summary>
    /// <param name="lifetime">The duration after which the cache item should expire if not accessed</param>
    /// <returns>Cache options configured for sliding expiration</returns>
    public static CacheOptions WithSlidingExpiration(Duration lifetime)
    {
        return new CacheOptions(Instant.MinValue, lifetime);
    }

    /// <summary>
    /// The absolute moment when the cache item expires
    /// </summary>
    public Instant Moment { get; }

    /// <summary>
    /// The sliding expiration duration
    /// </summary>
    public Duration Lifetime { get; }

    private CacheOptions(Instant moment, Duration lifetime)
    {
        Moment = moment;
        Lifetime = lifetime;
    }
}
