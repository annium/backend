using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.Internal;

namespace Annium.Extensions.Execution.Internal
{
    // ReSharper disable once UnusedTypeParameter
    internal class SequentialBackgroundExecutor<T> : IBackgroundExecutor
    {
        public bool IsAvailable => Volatile.Read(ref _isAvailable) == 1;
        private int Count => _taskReader.CanCount ? _taskReader.Count : -1;
        private int _isAvailable = 1;
        private int _isStarted;
        private readonly ChannelWriter<Delegate> _taskWriter;
        private readonly ChannelReader<Delegate> _taskReader;
        private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);
        private readonly CancellationTokenSource _cts = new();

        public void Schedule(Action task) => ScheduleTask(task);
        public void Schedule(Func<ValueTask> task) => ScheduleTask(task);

        public void TrySchedule(Action task) => TryScheduleTask(task);
        public void TrySchedule(Func<ValueTask> task) => TryScheduleTask(task);

        public SequentialBackgroundExecutor()
        {
            var taskChannel = Channel.CreateUnbounded<Delegate>(new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = true
            });
            _taskWriter = taskChannel.Writer;
            _taskReader = taskChannel.Reader;
        }

        public void Start(CancellationToken ct = default)
        {
            EnsureAvailable();

            if (Interlocked.CompareExchange(ref _isStarted, 1, 0) == 1)
                throw new InvalidOperationException("Executor is already running");

            // change to state to unavailable
            ct.Register(Stop);

            this.Trace("run");
            _runTask = Task.Run(Run, CancellationToken.None).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            this.Trace("start");
            EnsureAvailable();
            Stop();
            this.Trace($"wait for {Count} task(s) to finish");
            await _runTask;
            this.Trace("wait for reader completion");
            await _taskReader.Completion;
            _cts.Dispose();
            this.Trace("done");
        }

        private async Task Run()
        {
            this.Trace("start");
            // normal mode - runs task immediately or waits for one
            while (Volatile.Read(ref _isAvailable) == 1)
            {
                try
                {
                    var task = await _taskReader.ReadAsync(_cts.Token);
                    await RunTask(task);
                }
                catch (ChannelClosedException)
                {
                    this.Trace("cancelled");
                    break;
                }
                catch (OperationCanceledException)
                {
                    this.Trace("cancelled");
                    break;
                }
            }

            // shutdown mode - runs only left tasks
            this.Trace($"run {Count} tasks left");
            while (true)
            {
                if (_taskReader.TryRead(out var task))
                    await RunTask(task);
                else
                    break;
            }

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
            lock (_taskWriter)
                if (!_taskWriter.TryWrite(task))
                    throw new InvalidOperationException("Task must have been scheduled");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask RunTask(Delegate task)
        {
            if (task is Action syncTask)
                await Task.Run(syncTask);
            else if (task is Func<ValueTask> asyncValueTask)
                await asyncValueTask().ConfigureAwait(false);
            else
                throw new NotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Stop()
        {
            this.Trace("start");
            Volatile.Write(ref _isAvailable, 0);
            _cts.Cancel();
            lock (_taskWriter)
                _taskWriter.Complete();
            this.Trace("done");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureAvailable()
        {
            if (Volatile.Read(ref _isAvailable) == 0)
                throw new InvalidOperationException("Executor is not available already");
        }
    }
}