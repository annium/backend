using System;
using NodaTime;

namespace Annium.Cache.Abstractions;

/// <summary>
/// The expiration strategy encoded by a <see cref="CacheOptions"/> instance.
/// </summary>
public enum CacheExpirationMode
{
    /// <summary>
    /// The item expires at a fixed absolute moment.
    /// </summary>
    Absolute,

    /// <summary>
    /// The item expires after a sliding inactivity window that is refreshed on each access.
    /// </summary>
    Sliding,
}

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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="moment"/> is <see cref="Instant.MinValue"/> (the sentinel for "no absolute expiration"), which would yield a permanently-expired entry.</exception>
    public static CacheOptions WithAbsoluteExpiration(Instant moment)
    {
        if (moment == Instant.MinValue)
            throw new ArgumentOutOfRangeException(nameof(moment), "Absolute expiration moment must be a real instant.");

        return new CacheOptions(CacheExpirationMode.Absolute, moment, Duration.Zero);
    }

    /// <summary>
    /// Creates cache options with sliding expiration using the specified lifetime
    /// </summary>
    /// <param name="lifetime">The duration after which the cache item should expire if not accessed</param>
    /// <returns>Cache options configured for sliding expiration</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lifetime"/> is not positive, which would yield an immediately-expired entry.</exception>
    public static CacheOptions WithSlidingExpiration(Duration lifetime)
    {
        if (lifetime <= Duration.Zero)
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Sliding expiration lifetime must be positive.");

        return new CacheOptions(CacheExpirationMode.Sliding, Instant.MinValue, lifetime);
    }

    /// <summary>
    /// The expiration strategy this options instance encodes
    /// </summary>
    public CacheExpirationMode Mode { get; }

    /// <summary>
    /// The absolute moment when the cache item expires (meaningful when <see cref="Mode"/> is <see cref="CacheExpirationMode.Absolute"/>)
    /// </summary>
    public Instant Moment { get; }

    /// <summary>
    /// The sliding expiration duration (meaningful when <see cref="Mode"/> is <see cref="CacheExpirationMode.Sliding"/>)
    /// </summary>
    public Duration Lifetime { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheOptions"/> class; construct through the factory members.
    /// </summary>
    /// <param name="mode">How the entry expires.</param>
    /// <param name="moment">Absolute expiration moment; meaningful for the absolute mode.</param>
    /// <param name="lifetime">Relative lifetime; meaningful for the sliding mode.</param>
    private CacheOptions(CacheExpirationMode mode, Instant moment, Duration lifetime)
    {
        Mode = mode;
        Moment = moment;
        Lifetime = lifetime;
    }
}
