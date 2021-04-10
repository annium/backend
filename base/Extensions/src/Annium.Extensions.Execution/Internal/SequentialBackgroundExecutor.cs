using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Extensions.Execution.Internal
{
    internal class SequentialBackgroundExecutor : IBackgroundExecutor
    {
        public bool IsAvailable => Volatile.Read(ref _isAvailable) == 1;
        private int _isAvailable = 1;
        private int _isRunning;
        private readonly BlockingCollection<Delegate> _tasks = new();
        private Task _runTask = Task.CompletedTask;

        public void Schedule(Action work) => ScheduleWork(work);
        public void Schedule(Func<Task> work) => ScheduleWork(work);

        public void Start(CancellationToken ct)
        {
            EnsureAvailable();

            if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
                throw new InvalidOperationException("Executor is already running");

            // change to state to unavailable
            ct.Register(Stop);

            _runTask = Task.Run(Run, CancellationToken.None);
        }

        public async ValueTask DisposeAsync()
        {
            EnsureAvailable();
            Stop();
            await _runTask;
            _tasks.Dispose();
        }

        private async Task Run()
        {
            while (Volatile.Read(ref _isAvailable) == 1 || _tasks.Count > 0)
            {
                var task = _tasks.Take();
                if (task is Action syncTask)
                    syncTask();
                else if (task is Func<Task> asyncTask)
                    await asyncTask();
                else
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ScheduleWork(Delegate work)
        {
            EnsureAvailable();
            _tasks.Add(work);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stop()
        {
            Volatile.Write(ref _isAvailable, 0);
            Volatile.Write(ref _isRunning, 0);
            _tasks.CompleteAdding();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureAvailable()
        {
            if (Volatile.Read(ref _isAvailable) == 0)
                throw new InvalidOperationException("Executor is not available already");
        }
    }
}