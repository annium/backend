using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time
{
    internal class TimeProvider : ITimeProvider
    {
        public Instant Now => SystemClock.Instance.GetCurrentInstant();
    }
}