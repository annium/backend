using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal class Scheduler : IScheduler, IAsyncDisposable
{
    private readonly ITimeProvider _timeProvider;
    private readonly IIntervalResolver _intervalResolver;
    private readonly CancellationTokenSource _cts = new();
    private readonly IDictionary<Func<Task>, Func<Instant, bool>> _handlers = new Dictionary<Func<Task>, Func<Instant, bool>>();
    private readonly Task _runTask;

    public Scheduler(
        ITimeProvider timeProvider,
        IIntervalResolver intervalResolver
    )
    {
        _timeProvider = timeProvider;
        _intervalResolver = intervalResolver;
        _runTask = Task.Run(() => Run(_cts.Token));
    }

    public IDisposable Schedule(Func<Task> handler, string interval)
    {
        var isMatch = _intervalResolver.GetMatcher(interval);
        _handlers.Add(handler, isMatch);

        return Disposable.Create(() => _handlers.Remove(handler));
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
                TimeSpan.FromMilliseconds(time.Millisecond),
                CancellationToken.None
            );

            // run handlers
            var instant = _timeProvider.Now;
            await Task.WhenAll(_handlers.Where(e => e.Value(instant)).ToArray().Select(e => e.Key()));
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();
        _handlers.Clear();
        await _runTask;
    }
}