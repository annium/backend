using System;

namespace Annium.Core.Primitives
{
    public static class DateTimeExtensions
    {
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
    }
}