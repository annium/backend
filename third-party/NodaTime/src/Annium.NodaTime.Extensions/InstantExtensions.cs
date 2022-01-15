using Annium.Core.Primitives;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class InstantExtensions
{
    public static Instant FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch + Duration.FromMinutes(minutes);

    public static long ToUnixTimeMinutes(this Instant m) =>
        m.Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    public static Instant FloorToSecond(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToSecond();

    public static Instant FloorToMinute(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToMinute();

    public static Instant FloorToHour(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToHour();

    public static Instant FloorToDay(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToDay();

    public static Instant FloorTo(this Instant m, Duration d) =>
        Instant.MinValue + (m - Instant.MinValue).FloorTo(d);

    public static Instant CeilToSecond(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToSecond();

    public static Instant CeilToMinute(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToMinute();

    public static Instant CeilToHour(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToHour();

    public static Instant CeilToDay(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToDay();

    public static Instant CeilTo(this Instant m, Duration d) =>
        Instant.MinValue + (m - Instant.MinValue).CeilTo(d);

    public static bool IsMidnight(this Instant m)
    {
        var d = m - Instant.MinValue;

        return d.Hours == 0 && d.Minutes == 0 && d.Seconds == 0 && d.SubsecondTicks == 0;
    }
}