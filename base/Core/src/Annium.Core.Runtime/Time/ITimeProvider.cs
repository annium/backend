using System;
using NodaTime;

namespace Annium.Core.Runtime.Time
{
    public interface ITimeProvider
    {
        Instant Now { get; }
        DateTime DateTimeNow { get; }
    }
}