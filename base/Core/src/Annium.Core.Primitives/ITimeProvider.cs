using System;
using NodaTime;

namespace Annium.Core.Primitives
{
    public interface ITimeProvider
    {
        Instant Now { get; }
        DateTime DateTimeNow { get; }
        long UnixMsNow { get; }
        long UnixSecondsNow { get; }
    }
}