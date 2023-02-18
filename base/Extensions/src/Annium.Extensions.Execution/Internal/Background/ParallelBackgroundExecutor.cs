using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Internal;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class ParallelBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    private int _taskCounter;
    private readonly ConcurrentBag<Delegate> _backlog = new();
    private readonly TaskCompletionSource<object> _tcs = new();

    protected override void HandleStart()
    {
        while (_backlog.TryTake(out var task)) ScheduleTask(task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ScheduleTaskCore(Delegate task)
    {
        if (IsStarted)
        {
            Interlocked.Increment(ref _taskCounter);
            RunTask(task).ContinueWith(CompleteTask);
        }
        else
        {
            _backlog.Add(task);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void HandleStop()
    {
        this.Trace($"isAvailable: {IsAvailable}, tasks: {_taskCounter}");
        TryFinish();
    }

    protected override async ValueTask HandleDisposeAsync()
    {
        this.Trace($"wait for {_taskCounter} task(s) to finish");
        await _tcs.Task;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompleteTask(Task task)
    {
        Interlocked.Decrement(ref _taskCounter);
        TryFinish();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void TryFinish()
    {
        if (!IsAvailable && Volatile.Read(ref _taskCounter) == 0)
            _tcs.TrySetResult(new object());
    }
}