using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Extensions.Reactive;
using Annium.Extensions.Reactive.Operators;
using Annium.Logging;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// In-memory implementation of IMessageBusSocket using channels for message passing.
/// </summary>
internal class InMemoryMessageBusSocket : IMessageBusSocket
{
    /// <summary>
    /// Cancellation token source for the observable stream.
    /// </summary>
    private readonly CancellationTokenSource _observableCts = new();

    /// <summary>
    /// The observable stream of incoming messages.
    /// </summary>
    private readonly IObservable<string> _observable;

    /// <summary>
    /// The channel writer for sending messages.
    /// </summary>
    private readonly ChannelWriter<string> _messageWriter;

    /// <summary>
    /// The channel reader for receiving messages.
    /// </summary>
    private readonly ChannelReader<string> _messageReader;

    /// <summary>
    /// The disposable box for managing cleanup.
    /// </summary>
    private readonly AsyncDisposableBox _disposable;

    /// <summary>
    /// The logger instance for this socket.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the InMemoryMessageBusSocket class.
    /// </summary>
    /// <param name="logger">The logger to use for this socket.</param>
    public InMemoryMessageBusSocket(ILogger logger)
    {
        var taskChannel = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = true,
            }
        );
        _messageWriter = taskChannel.Writer;
        _messageReader = taskChannel.Reader;

        _observable = ObservableExt
            .StaticSyncInstance<string>(CreateObservableAsync, _observableCts.Token, logger)
            .TrackCompletion(logger);
        _disposable = Disposable.AsyncBox(logger);
        _logger = logger;
    }

    /// <summary>
    /// Sends a string message through the socket.
    /// </summary>
    /// <param name="message">The message string to send.</param>
    /// <returns>An observable that completes when the message is sent.</returns>
    public IObservable<Unit> Send(string message)
    {
        lock (_messageWriter)
            if (!_messageWriter.TryWrite(message))
                throw new InvalidOperationException("Message must have been written");

        return Observable.Return(Unit.Default);
    }

    /// <summary>
    /// Subscribes an observer to the message stream.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>A disposable that can be used to unsubscribe.</returns>
    public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

    /// <summary>
    /// Creates the observable stream that reads messages from the channel.
    /// </summary>
    /// <param name="ctx">The observer context for the stream.</param>
    /// <returns>A task that returns a cleanup function.</returns>
    private async Task<Func<Task>> CreateObservableAsync(ObserverContext<string> ctx)
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
        catch (OperationCanceledException) { }
        catch (ChannelClosedException) { }
        catch (Exception e)
        {
            ctx.OnError(e);
        }

        return () => Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously disposes the socket and its resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _observableCts.CancelAsync();
        await _observable.WhenCompletedAsync(_logger);

        await _disposable.DisposeAsync();
    }
}
