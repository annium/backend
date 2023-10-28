using NodaTime;

namespace Annium.Cache.Abstractions;

public sealed record CacheOptions
{
    public static CacheOptions WithAbsoluteExpiration(Instant moment)
    {
        return new CacheOptions(moment, Duration.Zero);
    }

    public static CacheOptions WithSlidingExpiration(Duration lifetime)
    {
        return new CacheOptions(Instant.MinValue, lifetime);
    }

    public Instant Moment { get; }
    public Duration Lifetime { get; }

    private CacheOptions(Instant moment, Duration lifetime)
    {
        Moment = moment;
        Lifetime = lifetime;
    }
}
