using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;

namespace Annium.Extensions.Execution.Internal
{
    // ReSharper disable once UnusedTypeParameter
    internal class ParallelBackgroundExecutor<T> : IBackgroundExecutor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task RunTask(Delegate task) => task switch
        {
            Action execute          => Task.Run(execute),
            Func<ValueTask> execute => Task.Run(async () => await execute().ConfigureAwait(false)),
            _                       => throw new NotSupportedException()
        };

        public bool IsAvailable => Volatile.Read(ref _isAvailable) == 1;
        private int _isAvailable = 1;
        private int _isStarted;
        private int _taskCounter;
        private readonly ConcurrentBag<Delegate> _backlog = new();
        private readonly TaskCompletionSource<object> _tcs = new();

        public void Schedule(Action task) => ScheduleTask(task);
        public void Schedule(Func<ValueTask> task) => ScheduleTask(task);
        public void TrySchedule(Action task) => TryScheduleTask(task);
        public void TrySchedule(Func<ValueTask> task) => TryScheduleTask(task);

        public void Start(CancellationToken ct = default)
        {
            EnsureAvailable();

            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
                throw new InvalidOperationException("Executor is already running");

            // change to state to unavailable
            ct.Register(Stop);

            while (_backlog.TryTake(out var task)) ScheduleTask(task);
        }

        public async ValueTask DisposeAsync()
        {
            this.Trace("start");
            EnsureAvailable();
            Stop();
            this.Trace($"wait for {_taskCounter} task(s) to finish");
            await _tcs.Task;
            this.Trace("done");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleTask(Delegate task)
        {
            EnsureAvailable();
            ScheduleTaskCore(task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryScheduleTask(Delegate task)
        {
            if (IsAvailable)
                ScheduleTaskCore(task);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleTaskCore(Delegate task)
        {
            if (Volatile.Read(ref _isStarted) == 1)
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
        private void CompleteTask(Task task)
        {
            Interlocked.Decrement(ref _taskCounter);
            TryFinish();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stop()
        {
            Volatile.Write(ref _isAvailable, 0);
            this.Trace($"isAvailable: {_isAvailable}, tasks: {_taskCounter}");
            TryFinish();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryFinish()
        {
            if (Volatile.Read(ref _isAvailable) == 0 && Volatile.Read(ref _taskCounter) == 0)
                _tcs.TrySetResult(new object());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureAvailable()
        {
            if (Volatile.Read(ref _isAvailable) == 0)
                throw new InvalidOperationException("Executor is not available already");
        }
    }
}