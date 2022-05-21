using System.Runtime.CompilerServices;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class LocalDateTimeExtensions
{
    private static readonly Instant UnixEpoch = Instant.FromUtc(1970, 1, 1, 0, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeMinutes(long minutes) =>
        UnixEpoch.Plus(Duration.FromMinutes(minutes)).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeSeconds(long seconds) =>
        UnixEpoch.Plus(Duration.FromSeconds(seconds)).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        UnixEpoch.Plus(Duration.FromMilliseconds(milliseconds)).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth GetYearMonth(this LocalDateTime m) =>
        new(m.Era, m.YearOfEra, m.Month, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(UnixEpoch).TotalMinutes.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(UnixEpoch).TotalSeconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(UnixEpoch).TotalMilliseconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime AlignToSecond(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, m.Minute, m.Second, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime AlignToMinute(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, m.Minute, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime AlignToHour(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, 0, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime AlignToDay(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, 0, 0, 0, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this LocalDateTime dateTime) =>
        dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
}