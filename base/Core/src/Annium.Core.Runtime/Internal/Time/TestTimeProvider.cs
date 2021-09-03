using System;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time
{
    internal class TestTimeProvider : IManagedTimeProvider
    {
        public event Action<Duration> NowChanged = delegate { };
        public Instant Now { get; private set; }
        public DateTime DateTimeNow { get; private set; }

        public void SetNow(Instant now)
        {
            var duration = now - Now;
            Now = now;
            DateTimeNow = now.ToDateTimeUtc();
            NowChanged(duration);
        }
    }
}