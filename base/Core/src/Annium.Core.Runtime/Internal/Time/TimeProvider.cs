using System;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time
{
    internal class TimeProvider : ITimeProvider
    {
        public Instant Now => SystemClock.Instance.GetCurrentInstant();
        public DateTime DateTimeNow => Now.ToDateTimeUtc();
    }
}