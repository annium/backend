using System;
using NodaTime;

namespace Annium.Cache.Abstractions;

public static class CacheOptionsExtensions
{
    public static Instant GetExpiresAt(this CacheOptions options, Instant now)
    {
        if (options.Moment != Instant.MinValue)
            return options.Moment;

        if (options.Lifetime != Duration.Zero)
            return now + options.Lifetime;

        throw new InvalidOperationException($"Failed to determine expiration time for options: {options}");
    }
}