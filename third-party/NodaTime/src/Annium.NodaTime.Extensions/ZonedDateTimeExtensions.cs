using System.Runtime.CompilerServices;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class ZonedDateTimeExtensions
{
    private static readonly Instant UnixEpoch = Instant.FromUtc(1970, 1, 1, 0, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeMinutes(long minutes) =>
        UnixEpoch.Plus(Duration.FromMinutes(minutes)).InUtc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeSeconds(long seconds) =>
        UnixEpoch.Plus(Duration.FromSeconds(seconds)).InUtc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        UnixEpoch.Plus(Duration.FromMilliseconds(milliseconds)).InUtc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth GetYearMonth(this ZonedDateTime m) =>
        new(m.Era, m.YearOfEra, m.Month, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this ZonedDateTime m) =>
        m.ToInstant().Minus(UnixEpoch).TotalMinutes.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this ZonedDateTime m) =>
        m.ToInstant().Minus(UnixEpoch).TotalSeconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this ZonedDateTime m) =>
        m.ToInstant().Minus(UnixEpoch).TotalMilliseconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime AlignToSecond(this ZonedDateTime m) =>
        new(new LocalDateTime(m.Year, m.Month, m.Day, m.Hour, m.Minute, m.Second, 0), m.Zone, m.Offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime AlignToMinute(this ZonedDateTime m) =>
        new(new LocalDateTime(m.Year, m.Month, m.Day, m.Hour, m.Minute, 0, 0), m.Zone, m.Offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime AlignToHour(this ZonedDateTime m) =>
        new(new LocalDateTime(m.Year, m.Month, m.Day, m.Hour, 0, 0, 0), m.Zone, m.Offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime AlignToDay(this ZonedDateTime m) =>
        new(new LocalDateTime(m.Year, m.Month, m.Day, 0, 0, 0, 0), m.Zone, m.Offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this ZonedDateTime dateTime) =>
        dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
}