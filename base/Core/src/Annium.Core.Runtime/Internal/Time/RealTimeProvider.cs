using System;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class RealTimeProvider : IInternalTimeProvider
{
    public Instant Now => SystemClock.Instance.GetCurrentInstant();
    public DateTime DateTimeNow => Now.ToDateTimeUtc();
    public long UnixMsNow => Now.ToUnixTimeMilliseconds();
    public long UnixSecondsNow => Now.ToUnixTimeSeconds();
}