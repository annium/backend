using Annium.Core.Primitives;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class InstantExtensions
{
    private static readonly Instant UnixEpoch = Instant.FromUtc(1970, 1, 1, 0, 0, 0);

    public static Instant FromUnixTimeMinutes(long minutes) =>
        UnixEpoch + Duration.FromMinutes(minutes);

    public static long ToUnixTimeMinutes(this Instant m) =>
        m.Minus(UnixEpoch).TotalMinutes.FloorInt64();
}