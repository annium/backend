using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class DurationExtensions
{
    public static Duration AlignToSecond(this Duration m) =>
        m - Duration.FromTicks(m.SubsecondTicks);

    public static Duration AlignToMinute(this Duration m) =>
        m - Duration.FromSeconds(m.Seconds) - Duration.FromTicks(m.SubsecondTicks);

    public static Duration AlignToHour(this Duration m) =>
        m - Duration.FromMinutes(m.Minutes) - Duration.FromSeconds(m.Seconds) - Duration.FromTicks(m.SubsecondTicks);

    public static Duration AlignToDay(this Duration m) =>
        m - Duration.FromHours(m.Hours) - Duration.FromMinutes(m.Minutes) - Duration.FromSeconds(m.Seconds) - Duration.FromTicks(m.SubsecondTicks);
}