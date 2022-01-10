using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class DurationExtensions
{
    public static Duration AlignToSecond(this Duration m) =>
        m.AlignTo(Duration.FromSeconds(1));

    public static Duration AlignToMinute(this Duration m) =>
        m.AlignTo(Duration.FromMinutes(1));

    public static Duration AlignToHour(this Duration m) =>
        m.AlignTo(Duration.FromHours(1));

    public static Duration AlignToDay(this Duration m) =>
        m.AlignTo(Duration.FromDays(1));

    public static Duration AlignTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;

        return Duration.FromTicks(mt - mt % dt);
    }
}