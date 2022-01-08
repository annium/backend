using System;

namespace Annium.Core.Primitives;

public static class DateTimeExtensions
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime FromUnixTimeMinutes(long minutes) =>
        UnixEpoch.AddMinutes(minutes);

    public static DateTime FromUnixTimeSeconds(long seconds) =>
        UnixEpoch.AddSeconds(seconds);

    public static DateTime FromUnixTimeMilliseconds(long milliseconds) =>
        UnixEpoch.AddSeconds(milliseconds);

    public static DateTime AlignToSecond(this DateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, m.Minute, m.Second, 0, m.Kind);

    public static DateTime AlignToMinute(this DateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, m.Minute, 0, 0, m.Kind);

    public static DateTime AlignToHour(this DateTime m) =>
        new(m.Year, m.Month, m.Day, m.Hour, 0, 0, 0, m.Kind);

    public static DateTime AlignToDay(this DateTime m) =>
        new(m.Year, m.Month, m.Day, 0, 0, 0, 0, m.Kind);

    public static DateTime InUtc(this DateTime m) =>
        DateTime.SpecifyKind(m, DateTimeKind.Utc);

    public static long ToUnixTimeMinutes(this DateTime m) =>
        (m.InUtc() - UnixEpoch).TotalMinutes.FloorInt64();

    public static long ToUnixTimeSeconds(this DateTime m) =>
        (m.InUtc() - UnixEpoch).TotalSeconds.FloorInt64();

    public static long ToUnixTimeMilliseconds(this DateTime m) =>
        (m.InUtc() - UnixEpoch).TotalMilliseconds.FloorInt64();
}