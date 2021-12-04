using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;

namespace Annium.Extensions.Execution.Internal.Background;

internal abstract class BackgroundExecutorBase : IBackgroundExecutor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static Task RunTask(Delegate task) => task switch
    {
        Action execute          => Task.Run(execute),
        Func<ValueTask> execute => Task.Run(async () => await execute().ConfigureAwait(false)),
        _                       => throw new NotSupportedException()
    };

    public bool IsAvailable => Volatile.Read(ref _isAvailable) == 1;
    protected bool IsStarted => Volatile.Read(ref _isStarted) == 1;

    private int _isAvailable = 1;
    private int _isStarted;

    public void Schedule(Action task)
    {
        ScheduleTask(task);
    }

    public void Schedule(Func<ValueTask> task)
    {
        ScheduleTask(task);
    }

    public bool TrySchedule(Action task)
    {
        return TryScheduleTask(task);
    }

    public bool TrySchedule(Func<ValueTask> task)
    {
        return TryScheduleTask(task);
    }

    public Task ExecuteAsync(Action task)
    {
        ScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task<T> ExecuteAsync<T>(Func<T> task)
    {
        ScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task ExecuteAsync(Func<ValueTask> task)
    {
        ScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task<T> ExecuteAsync<T>(Func<ValueTask<T>> task)
    {
        ScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task TryExecuteAsync(Action task)
    {
        TryScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task<T> TryExecuteAsync<T>(Func<T> task)
    {
        TryScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task TryExecuteAsync(Func<ValueTask> task)
    {
        TryScheduleTask(task);
        throw new NotImplementedException();
    }

    public Task<T> TryExecuteAsync<T>(Func<ValueTask<T>> task)
    {
        TryScheduleTask(task);
        throw new NotImplementedException();
    }

    public void Start(CancellationToken ct = default)
    {
        EnsureAvailable();

        if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
            throw new InvalidOperationException("Executor is already running");

        // change to state to unavailable
        ct.Register(Stop);

        this.Trace("run");
        HandleStart();
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");
        EnsureAvailable();
        Stop();
        await HandleDisposeAsync();
        this.Trace("done");
    }

    protected abstract void HandleStart();
    protected abstract void ScheduleTaskCore(Delegate task);
    protected abstract void HandleStop();
    protected abstract ValueTask HandleDisposeAsync();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ScheduleTask(Delegate task)
    {
        EnsureAvailable();
        ScheduleTaskCore(task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryScheduleTask(Delegate task)
    {
        if (!IsAvailable)
            return false;

        ScheduleTaskCore(task);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureAvailable()
    {
        if (Volatile.Read(ref _isAvailable) == 0)
            throw new InvalidOperationException("Executor is not available already");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stop()
    {
        this.Trace("start");
        Volatile.Write(ref _isAvailable, 0);
        HandleStop();
        this.Trace("done");
    }
}