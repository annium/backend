using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Extensions.Jobs
{
    internal class Scheduler : IScheduler, IDisposable
    {
        private readonly Func<Instant> getInstant;

        private readonly IIntervalResolver intervalResolver;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly IDictionary<Func<Task>, Func<Instant, bool>> handlers =
            new Dictionary<Func<Task>, Func<Instant, bool>>();

        public Scheduler(
            Func<Instant> getInstant,
            IIntervalResolver intervalResolver
        )
        {
            this.getInstant = getInstant;
            this.intervalResolver = intervalResolver;
            Run(cts.Token).GetAwaiter();
        }

        public Action Schedule(Func<Task> handler, string interval)
        {
            var isMatch = intervalResolver.GetMatcher(interval);
            handlers.Add(handler, isMatch);

            return () => handlers.Remove(handler);
        }

        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // wait till next minute start
                var time = getInstant().InUtc();
                await Task.Delay(
                    TimeSpan.FromMinutes(1) -
                    TimeSpan.FromSeconds(time.Second) -
                    TimeSpan.FromMilliseconds(time.Millisecond)
                );

                // run handlers
                var instant = getInstant();
                await Task.WhenAll(handlers.Where(e => e.Value(instant)).ToArray().Select(e => e.Key()));
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    cts.Cancel();
                    cts.Dispose();
                    handlers.Clear();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}