using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Threading.Tasks;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal
{
    internal class Scheduler : IScheduler, IDisposable
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IIntervalResolver _intervalResolver;
        private readonly CancellationTokenSource _cts = new();
        private readonly IDictionary<Func<Task>, Func<Instant, bool>> _handlers = new Dictionary<Func<Task>, Func<Instant, bool>>();

        public Scheduler(
            ITimeProvider timeProvider,
            IIntervalResolver intervalResolver
        )
        {
            _timeProvider = timeProvider;
            _intervalResolver = intervalResolver;
            Run(_cts.Token).Await();
        }

        public Action Schedule(Func<Task> handler, string interval)
        {
            var isMatch = _intervalResolver.GetMatcher(interval);
            _handlers.Add(handler, isMatch);

            return () => _handlers.Remove(handler);
        }

        private async Task Run(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // wait till next minute start
                var time = _timeProvider.Now.InUtc();
                await Task.Delay(
                    TimeSpan.FromMinutes(1) -
                    TimeSpan.FromSeconds(time.Second) -
                    TimeSpan.FromMilliseconds(time.Millisecond)
                );

                // run handlers
                var instant = _timeProvider.Now;
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