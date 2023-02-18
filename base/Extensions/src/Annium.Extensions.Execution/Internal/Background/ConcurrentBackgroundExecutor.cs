using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Internal;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class ConcurrentBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    private int Count => _taskReader.CanCount ? _taskReader.Count : -1;
    private readonly ChannelWriter<Delegate> _taskWriter;
    private readonly ChannelReader<Delegate> _taskReader;
    private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _gate;
    private int _taskCounter;
    private readonly TaskCompletionSource<object> _tcs = new();

    public ConcurrentBackgroundExecutor(int parallelism)
    {
        _gate = new SemaphoreSlim(parallelism, parallelism);
        var taskChannel = Channel.CreateUnbounded<Delegate>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = true,
            SingleWriter = false,
            SingleReader = true
        });
        _taskWriter = taskChannel.Writer;
        _taskReader = taskChannel.Reader;
    }

    protected override void HandleStart()
    {
        _runTask = Task.Run(Run, CancellationToken.None).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ScheduleTaskCore(Delegate task)
    {
        lock (_taskWriter)
        {
            Interlocked.Increment(ref _taskCounter);
            if (!_taskWriter.TryWrite(task))
                throw new InvalidOperationException("Task must have been scheduled");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void HandleStop()
    {
        _cts.Cancel();
        lock (_taskWriter)
            _taskWriter.Complete();

        this.Trace($"isAvailable: {IsAvailable}, tasks: {_taskCounter}");
        TryFinish();
    }

    protected override async ValueTask HandleDisposeAsync()
    {
        this.Trace($"wait for {Count} task(s) to be scheduled");
        await _runTask;
        this.Trace("wait for reader completion");
        await _taskReader.Completion;
        _cts.Dispose();

        this.Trace($"wait for {_taskCounter} task(s) to finish");
        await _tcs.Task;

        _gate.Dispose();
    }

    private async Task Run()
    {
        this.Trace("start");

        // normal mode - runs task immediately or waits for one
        while (IsAvailable)
        {
            try
            {
                await _gate.WaitAsync();
                var task = await _taskReader.ReadAsync(_cts.Token);
                RunTask(task).ContinueWith(CompleteTask).GetAwaiter();
            }
            catch (ChannelClosedException)
            {
                this.Trace("channel closed");
                _gate.Release();
                break;
            }
            catch (OperationCanceledException)
            {
                this.Trace("operation canceled");
                _gate.Release();
                break;
            }
        }

        // shutdown mode - runs only left tasks
        this.Trace($"run {Count} tasks left");
        while (true)
        {
            await _gate.WaitAsync();

            if (!_taskReader.TryRead(out var task))
                break;

            RunTask(task).ContinueWith(CompleteTask).GetAwaiter();
        }

        this.Trace("done");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompleteTask(Task _)
    {
        Interlocked.Decrement(ref _taskCounter);
        _gate.Release();
        TryFinish();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void TryFinish()
    {
        if (!IsAvailable && Volatile.Read(ref _taskCounter) == 0)
            _tcs.TrySetResult(new object());
    }
}