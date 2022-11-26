using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal class Scheduler : IScheduler, IAsyncDisposable, ILogSubject<Scheduler>
{
    public ILogger<Scheduler> Logger { get; }
    private readonly ITimeProvider _timeProvider;
    private readonly IIntervalResolver _intervalResolver;
    private readonly IDictionary<Func<Task>, Func<Instant, bool>> _handlers = new Dictionary<Func<Task>, Func<Instant, bool>>();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _runTask;
    private bool _isDisposed;

    public Scheduler(
        ITimeProvider timeProvider,
        IIntervalResolver intervalResolver,
        ILogger<Scheduler> logger
    )
    {
        Logger = logger;
        _timeProvider = timeProvider;
        _intervalResolver = intervalResolver;

        _runTask = Task.Run(async () => await Run(_cts.Token));
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
            try
            {
                // wait till next minute start
                var time = _timeProvider.Now.InUtc();
                // TODO: implement subtraction of milliseconds from resolved time
                await Task.Delay(
                    TimeSpan.FromMinutes(1) -
                    TimeSpan.FromSeconds(time.Second) -
                    TimeSpan.FromMilliseconds(time.Millisecond),
                    ct
                );

                // run handlers
                var instant = _timeProvider.Now;
                await Task.WhenAll(_handlers.Where(e => e.Value(instant)).ToArray().Select(e => e.Key()));
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        this.Log().Trace("cancel run");
        _cts.Cancel();
        _cts.Dispose();

        this.Log().Trace("clear handlers");
        _handlers.Clear();

        this.Log().Trace("await run task");
        await _runTask;

        this.Log().Trace("done");
    }
}