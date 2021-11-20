using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class LocalDateTimeExtensions
{
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
}