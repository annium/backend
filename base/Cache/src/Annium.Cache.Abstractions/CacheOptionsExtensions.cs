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
    public static Instant GetExpiresAt(this CacheOptions options, Instant now)
    {
        if (options.Moment != Instant.MinValue)
            return options.Moment;

        if (options.Lifetime != Duration.Zero)
            return now + options.Lifetime;

        throw new InvalidOperationException($"Failed to determine expiration time for options: {options}");
    }
}
