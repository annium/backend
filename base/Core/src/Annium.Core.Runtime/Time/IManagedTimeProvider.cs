using NodaTime;

namespace Annium.Core.Runtime.Time
{
    public interface IManagedTimeProvider : ITimeProvider
    {
        void SetNow(Instant now);
    }
}