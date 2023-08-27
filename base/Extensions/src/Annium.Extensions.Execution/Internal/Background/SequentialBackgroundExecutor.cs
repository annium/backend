using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Debug;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class SequentialBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    private int Count => _taskReader.CanCount ? _taskReader.Count : -1;
    private readonly ChannelWriter<Delegate> _taskWriter;
    private readonly ChannelReader<Delegate> _taskReader;
    private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);
    private readonly CancellationTokenSource _cts = new();

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

    protected override void HandleStart()
    {
        _runTask = Task.Run(Run, CancellationToken.None).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void HandleStop()
    {
        _cts.Cancel();
        _taskWriter.Complete();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ScheduleTaskCore(Delegate task)
    {
        if (!_taskWriter.TryWrite(task))
            throw new InvalidOperationException("Task must have been scheduled");
    }

    protected override async ValueTask HandleDisposeAsync()
    {
        this.TraceOld($"wait for {Count} task(s) to finish");
        await _runTask;
        this.TraceOld("wait for reader completion");
        await _taskReader.Completion.ConfigureAwait(false);
        _cts.Dispose();
    }

    private async Task Run()
    {
        this.TraceOld("start");
        // normal mode - runs task immediately or waits for one
        while (IsAvailable)
        {
            try
            {
                var task = await _taskReader.ReadAsync(_cts.Token);
                await Helper.RunTaskInForeground(task, Cts.Token);
            }
            catch (ChannelClosedException)
            {
                this.TraceOld("channel closed");
                break;
            }
            catch (OperationCanceledException)
            {
                this.TraceOld("operation canceled");
                break;
            }
        }

        // shutdown mode - runs only left tasks
        this.TraceOld($"run {Count} tasks left");
        while (true)
        {
            if (_taskReader.TryRead(out var task))
                await Helper.RunTaskInForeground(task, Cts.Token);
            else
                break;
        }

        this.TraceOld("done");
    }
}