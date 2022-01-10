using Annium.Core.Primitives;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class InstantExtensions
{
    public static Instant FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch + Duration.FromMinutes(minutes);

    public static long ToUnixTimeMinutes(this Instant m) =>
        m.Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    public static Instant AlignToSecond(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).AlignToSecond();

    public static Instant AlignToMinute(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).AlignToMinute();

    public static Instant AlignToHour(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).AlignToHour();

    public static Instant AlignToDay(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).AlignToDay();

    public static Instant AlignTo(this Instant m, Duration d) =>
        Instant.MinValue + (m - Instant.MinValue).AlignTo(d);
}