using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Extensions.Execution;
using Annium.Logging.Abstractions;
using Annium.NodaTime.Extensions;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

internal class Scheduler : IScheduler, IAsyncDisposable, ILogSubject<Scheduler>
{
    public ILogger<Scheduler> Logger { get; }
    private readonly ITimeProvider _timeProvider;
    private readonly IIntervalParser _intervalParser;
    private readonly CancellationTokenSource _cts = new();
    private bool _isDisposed;
    private readonly IBackgroundExecutor _executor;

    public Scheduler(
        ITimeProvider timeProvider,
        IIntervalParser intervalParser,
        ILogger<Scheduler> logger
    )
    {
        Logger = logger;
        _timeProvider = timeProvider;
        _intervalParser = intervalParser;

        _executor = Executor.Background.Parallel<Scheduler>();
        _executor.Start();
    }

    public IDisposable Schedule(Func<Task> handler, string interval)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
        var resolveDelay = _intervalParser.GetDelayResolver(interval);

        // warm up resolver
        resolveDelay(_timeProvider.Now.InUtc().LocalDateTime);

        _executor.Schedule(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                var time = _timeProvider.Now.InUtc().LocalDateTime;
                var delay = resolveDelay(time.CeilToSecond());
                var ms = Duration.FromMilliseconds(1000 - time.Millisecond);
                var total = (delay + ms).ToTimeSpan();

                await Task.Delay(total, cts.Token);

                if (!cts.IsCancellationRequested)
                    await handler();
            }
        });

        return Disposable.Create(() => cts.Cancel());
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        this.Log().Trace("cancel run");
        _cts.Cancel();
        _cts.Dispose();

        this.Log().Trace("await executor");
        await _executor.DisposeAsync();

        this.Log().Trace("done");
    }
}