using NodaTime;

namespace Annium.Core.Runtime.Time
{
    public static class ManagedTimeProviderExtensions
    {
        public static void AddSecond(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromSeconds(1L));

        public static void AddSeconds(this IManagedTimeProvider timeProvider, long seconds) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromSeconds(seconds));

        public static void AddMinute(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromMinutes(1L));

        public static void AddMinutes(this IManagedTimeProvider timeProvider, long minutes) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromMinutes(minutes));

        public static void AddHour(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromHours(1L));

        public static void AddHours(this IManagedTimeProvider timeProvider, long hours) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromHours(hours));

        public static void AddDay(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromDays(1L));

        public static void AddDays(this IManagedTimeProvider timeProvider, long days) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromDays(days));
    }
}