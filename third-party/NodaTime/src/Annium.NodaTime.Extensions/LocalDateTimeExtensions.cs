using System;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class LocalDateTimeExtensions
{
    private static readonly LocalDateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, 0);

    public static LocalDateTime FromUnixTimeMinutes(long minutes) =>
        UnixEpoch.PlusMinutes(minutes);

    public static LocalDateTime FromUnixTimeSeconds(long seconds) =>
        UnixEpoch.PlusSeconds(seconds);

    public static LocalDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        UnixEpoch.PlusMilliseconds(milliseconds);

    public static LocalDateTime AlignToSecond(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, m.Minute, m.Second, 0);

    public static LocalDateTime AlignToMinute(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, m.Minute, 0, 0);

    public static LocalDateTime AlignToHour(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, 0, 0, 0);

    public static LocalDateTime AlignToDay(this LocalDateTime m) =>
        new(m.Year, m.Month, m.Day, 0, 0, 0, 0);

    public static bool IsMidnight(this LocalDateTime dateTime)
    {
        return dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0 && dateTime.Millisecond == 0;
    }

    public static long ToUnixTimeMinutes(this LocalDateTime m) =>
        (long)Math.Floor((m - UnixEpoch).ToDuration().TotalSeconds);

    public static long ToUnixTimeSeconds(this LocalDateTime m) =>
        (long)Math.Floor((m - UnixEpoch).ToDuration().TotalSeconds);

    public static long ToUnixTimeMilliseconds(this LocalDateTime m) =>
        (long)Math.Floor((m - UnixEpoch).ToDuration().TotalMilliseconds);
}