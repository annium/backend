using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;

namespace Annium.Extensions.Execution.Internal.Background;

internal abstract class BackgroundExecutorBase : IBackgroundExecutor
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static Task RunTask(Delegate task) => task switch
    {
        Action execute          => Task.Run(execute),
        Func<ValueTask> execute => Task.Run(async () => await execute().ConfigureAwait(false)),
        Func<Task> execute      => Task.Run(async () => await execute().ConfigureAwait(false)),
        _                       => throw new NotSupportedException()
    };

    public bool IsAvailable => _state is State.Created or State.Started;
    protected bool IsStarted => _state is State.Started;

    private State _state = State.Created;

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
        var tcs = new TaskCompletionSource();
        ScheduleTask(() =>
        {
            try
            {
                task();
                tcs.SetResult();
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });

        return tcs.Task;
    }

    public Task<T> ExecuteAsync<T>(Func<T> task)
    {
        var tcs = new TaskCompletionSource<T>();
        ScheduleTask(() =>
        {
            try
            {
                tcs.SetResult(task());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });

        return tcs.Task;
    }

    public Task ExecuteAsync(Func<ValueTask> task)
    {
        var tcs = new TaskCompletionSource();
        ScheduleTask(async () =>
        {
            try
            {
                await task();
                tcs.SetResult();
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });

        return tcs.Task;
    }

    public Task<T> ExecuteAsync<T>(Func<ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<T>();
        ScheduleTask(async () =>
        {
            try
            {
                tcs.SetResult(await task());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });

        return tcs.Task;
    }

    public Task TryExecuteAsync(Action task)
    {
        var tcs = new TaskCompletionSource();
        var scheduled = TryScheduleTask(() =>
        {
            try
            {
                task();
                tcs.SetResult();
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetException(UnavailableException());

        return tcs.Task;
    }

    public Task<T> TryExecuteAsync<T>(Func<T> task)
    {
        var tcs = new TaskCompletionSource<T>();
        var scheduled = TryScheduleTask(() =>
        {
            try
            {
                tcs.SetResult(task());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetException(UnavailableException());

        return tcs.Task;
    }

    public Task TryExecuteAsync(Func<ValueTask> task)
    {
        var tcs = new TaskCompletionSource();
        var scheduled = TryScheduleTask(async () =>
        {
            try
            {
                await task();
                tcs.SetResult();
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetException(UnavailableException());

        return tcs.Task;
    }

    public Task<T> TryExecuteAsync<T>(Func<ValueTask<T>> task)
    {
        var tcs = new TaskCompletionSource<T>();
        var scheduled = TryScheduleTask(async () =>
        {
            try
            {
                tcs.SetResult(await task());
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (!scheduled)
            tcs.SetException(UnavailableException());

        return tcs.Task;
    }

    public void Start(CancellationToken ct = default)
    {
        EnsureAvailable();

        var sl = new SpinLock();
        var lockTaken = false;
        try
        {
            sl.Enter(ref lockTaken);
            if (_state is not State.Created)
                throw new InvalidOperationException("Executor has already started");
            _state = State.Started;
        }
        finally
        {
            if (lockTaken)
                sl.Exit();
        }

        // change to state to unavailable
        ct.Register(Stop);

        this.Trace("run");
        HandleStart();
    }

    public async ValueTask DisposeAsync()
    {
        var sl = new SpinLock();
        var lockTaken = false;
        try
        {
            sl.Enter(ref lockTaken);
            if (_state is State.Disposed)
            {
                this.Trace("already disposed");
                return;
            }

            _state = State.Disposed;
        }
        finally
        {
            if (lockTaken)
                sl.Exit();
        }

        this.Trace("start");
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
        if (!IsAvailable)
            throw new InvalidOperationException("Executor is not available already");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stop()
    {
        var sl = new SpinLock();
        var lockTaken = false;
        try
        {
            sl.Enter(ref lockTaken);
            if (_state is State.Stopped)
            {
                this.Trace("already stopped");
                return;
            }

            _state = State.Stopped;
        }
        finally
        {
            if (lockTaken)
                sl.Exit();
        }

        this.Trace("start");
        HandleStop();
        this.Trace("done");
    }

    private static InvalidOperationException UnavailableException() =>
        new("Executor is not available already");

    private enum State : byte
    {
        Created,
        Started,
        Stopped,
        Disposed
    }
}