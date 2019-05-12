using System;
using NodaTime;

namespace Annium.Extensions.Jobs
{
    public interface IIntervalResolver
    {
        Func<Instant, bool> GetMatcher(string interval);
    }
}