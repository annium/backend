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
        private readonly Func<Instant> _getInstant;

        private readonly IIntervalResolver _intervalResolver;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly IDictionary<Func<Task>, Func<Instant, bool>> _handlers =
            new Dictionary<Func<Task>, Func<Instant, bool>>();

        public Scheduler(
            Func<Instant> getInstant,
            IIntervalResolver intervalResolver
        )
        {
            _getInstant = getInstant;
            _intervalResolver = intervalResolver;
            Run(_cts.Token).GetAwaiter();
        }

        public Action Schedule(Func<Task> handler, string interval)
        {
            var isMatch = _intervalResolver.GetMatcher(interval);
            _handlers.Add(handler, isMatch);

            return () => _handlers.Remove(handler);
        }

        private async Task Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // wait till next minute start
                var time = _getInstant().InUtc();
                await Task.Delay(
                    TimeSpan.FromMinutes(1) -
                    TimeSpan.FromSeconds(time.Second) -
                    TimeSpan.FromMilliseconds(time.Millisecond)
                );

                // run handlers
                var instant = _getInstant();
                await Task.WhenAll(_handlers.Where(e => e.Value(instant)).ToArray().Select(e => e.Key()));
            }
        }

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _handlers.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}