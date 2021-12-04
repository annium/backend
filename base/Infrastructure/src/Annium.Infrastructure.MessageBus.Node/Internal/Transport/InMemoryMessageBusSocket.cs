using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

internal class InMemoryMessageBusSocket : IMessageBusSocket
{
    private readonly CancellationTokenSource _observableCts = new();
    private readonly IObservable<string> _observable;
    private readonly ChannelWriter<string> _messageWriter;
    private readonly ChannelReader<string> _messageReader;
    private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

    public InMemoryMessageBusSocket()
    {
        var taskChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            AllowSynchronousContinuations = true,
            SingleWriter = false,
            SingleReader = true
        });
        _messageWriter = taskChannel.Writer;
        _messageReader = taskChannel.Reader;

        _observable = ObservableExt.StaticSyncInstance<string>(CreateObservable, _observableCts.Token);
    }

    public IObservable<Unit> Send(string message)
    {
        lock (_messageWriter)
            if (!_messageWriter.TryWrite(message))
                throw new InvalidOperationException("Message must have been written");

        return Observable.Return(Unit.Default);
    }

    public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

    private async Task<Func<Task>> CreateObservable(ObserverContext<string> ctx)
    {
        try
        {
            while (!ctx.Ct.IsCancellationRequested)
            {
                var message = await _messageReader.ReadAsync(ctx.Ct);

                ctx.OnNext(message);
            }
        }
        // token was canceled
        catch (OperationCanceledException)
        {
        }
        catch (ChannelClosedException)
        {
        }
        catch (Exception e)
        {
            ctx.OnError(e);
        }

        return () => Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _observableCts.Cancel();
        await _observable.WhenCompleted();

        await _disposable.DisposeAsync();
    }
}