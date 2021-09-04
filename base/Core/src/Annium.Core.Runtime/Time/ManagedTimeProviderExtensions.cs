using NodaTime;

namespace Annium.Core.Runtime.Time
{
    public static class ManagedTimeProviderExtensions
    {
        public static void AddMinute(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromMinutes(1));

        public static void AddMinutes(this IManagedTimeProvider timeProvider, int minutes) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromMinutes(minutes));

        public static void AddHour(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromHours(1));

        public static void AddHours(this IManagedTimeProvider timeProvider, int hours) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromHours(hours));

        public static void AddDay(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromDays(1));

        public static void AddDays(this IManagedTimeProvider timeProvider, int days) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromDays(days));
    }
}