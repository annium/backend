using System;
using NodaTime;

namespace Annium.Core.Runtime.Time
{
    public interface IManagedTimeProvider : ITimeProvider
    {
        event Action<Duration> NowChanged;
        void SetNow(Instant now);
    }
}