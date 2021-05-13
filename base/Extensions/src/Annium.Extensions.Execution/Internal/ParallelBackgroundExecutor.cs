using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Internal
{
    internal class ParallelBackgroundExecutor : IBackgroundExecutor
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task RunWork(Delegate work) => work switch
        {
            Action execute     => Task.Run(execute),
            Func<Task> execute => Task.Run(execute),
            _                  => throw new NotSupportedException()
        };

        public bool IsAvailable => Volatile.Read(ref _isAvailable) == 1;
        private int _isAvailable = 1;
        private int _isRunning;
        private int _workCounter;
        private readonly ConcurrentBag<Delegate> _backlog = new();
        private readonly TaskCompletionSource<object> _tcs = new();

        public void Schedule(Action task) => ScheduleWork(task);
        public void Schedule(Func<Task> task) => ScheduleWork(task);

        public void Start(CancellationToken ct = default)
        {
            EnsureAvailable();

            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
                throw new InvalidOperationException("Executor is already running");

            // change to state to unavailable
            ct.Register(Stop);

            while (_backlog.TryTake(out var work)) ScheduleWork(work);
        }

        public async ValueTask DisposeAsync()
        {
            EnsureAvailable();
            Stop();
            await _tcs.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleWork(Delegate work)
        {
            EnsureAvailable();
            if (Volatile.Read(ref _isRunning) == 1)
            {
                Interlocked.Increment(ref _workCounter);
                RunWork(work).ContinueWith(CompleteWork);
            }
            else
                _backlog.Add(work);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CompleteWork(Task _)
        {
            Interlocked.Decrement(ref _workCounter);
            TryFinish();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stop()
        {
            Volatile.Write(ref _isAvailable, 0);
            Volatile.Write(ref _isRunning, 0);
            TryFinish();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryFinish()
        {
            if (Volatile.Read(ref _isAvailable) == 0 && Volatile.Read(ref _workCounter) == 0)
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