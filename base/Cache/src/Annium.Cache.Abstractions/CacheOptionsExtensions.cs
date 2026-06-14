using System;
using NodaTime;

namespace Annium.Cache.Abstractions;

/// <summary>
/// Extension methods for CacheOptions
/// </summary>
public static class CacheOptionsExtensions
{
    /// <summary>
    /// Calculates the absolute expiration time based on cache options and current time
    /// </summary>
    /// <param name="options">The cache options</param>
    /// <param name="now">The current time</param>
    /// <returns>The calculated absolute expiration time</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="options"/> has an unrecognized expiration <see cref="CacheOptions.Mode"/>.</exception>
    public static Instant GetExpiresAt(this CacheOptions options, Instant now)
    {
        return options.Mode switch
        {
            CacheExpirationMode.Absolute => options.Moment,
            CacheExpirationMode.Sliding => now + options.Lifetime,
            _ => throw new InvalidOperationException($"Failed to determine expiration time for options: {options}"),
        };
    }
}
