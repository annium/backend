using NodaTime;

namespace Annium.Core.Runtime.Time
{
    public static class ManagedTimeProviderExtensions
    {
        public static void AddMinute(this IManagedTimeProvider timeProvider) =>
            timeProvider.SetNow(timeProvider.Now + Duration.FromMinutes(1));
    }
}