using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Diagnostics.Debug;

namespace Annium.Extensions.Execution.Internal
{
    // ReSharper disable once UnusedTypeParameter
    internal class SequentialBackgroundExecutor<T> : IBackgroundExecutor
    {
        public bool IsAvailable => Volatile.Read(ref _isAvailable) == 1;
        private int _isAvailable = 1;
        private int _isStarted;
        private readonly BlockingCollection<Delegate> _tasks = new();
        private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);
        private readonly CancellationTokenSource _cts = new();

        public void Schedule(Action task) => ScheduleTask(task);
        public void Schedule(Func<Task> task) => ScheduleTask(task);

        public void TrySchedule(Action task) => TryScheduleTask(task);
        public void TrySchedule(Func<Task> task) => TryScheduleTask(task);

        public void Start(CancellationToken ct = default)
        {
            EnsureAvailable();

            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) != 0)
                throw new InvalidOperationException("Executor is already running");

            // change to state to unavailable
            ct.Register(Stop);

            this.Trace(() => "run");
            _runTask = Task.Run(Run, CancellationToken.None).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            this.Trace(() => "start");
            EnsureAvailable();
            Stop();
            var tasksCount = _tasks.Count;
            this.Trace(() => $"wait for {tasksCount} task(s) to finish");
            await _runTask;
            _tasks.Dispose();
            _cts.Dispose();
            this.Trace(() => "done");
        }

        private async Task Run()
        {
            this.Trace(() => "start");
            // normal mode - runs task immediately or waits for one
            while (Volatile.Read(ref _isAvailable) == 1)
            {
                try
                {
                    this.Trace(() => "wait for task");
                    var task = _tasks.Take(_cts.Token);
                    await RunTask(task);
                }
                catch (OperationCanceledException)
                {
                    this.Trace(() => "cancelled");
                    break;
                }
            }

            // shutdown mode - runs only left tasks
            _tasks.CompleteAdding();
            while (_tasks.Count > 0)
            {
                this.Trace(() => "get task");
                var task = _tasks.Take();
                await RunTask(task);
            }

            this.Trace(() => "done");
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
            _tasks.Add(task);
            this.Trace(() => $"added {task}, {_tasks.Count} tasks left");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async Task RunTask(Delegate task)
        {
            this.Trace(() => $"task {task.GetId()}: start, {_tasks.Count} tasks left");
            if (task is Action syncTask)
                await Task.Run(syncTask);
            else if (task is Func<Task> asyncTask)
                await asyncTask().ConfigureAwait(false);
            else
                throw new NotSupportedException();
            this.Trace(() => $"task {task.GetId()}: complete");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stop()
        {
            Volatile.Write(ref _isAvailable, 0);
            _cts.Cancel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureAvailable()
        {
            if (Volatile.Read(ref _isAvailable) == 0)
                throw new InvalidOperationException("Executor is not available already");
        }
    }
}