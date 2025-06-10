using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Reactive;
using Annium.Extensions.Reactive.Operators;
using Annium.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

/// <summary>
/// NetMQ-based implementation of IMessageBusSocket for network communication using publisher-subscriber pattern.
/// </summary>
// ReSharper disable once InconsistentNaming
internal class NetMQMessageBusSocket : IMessageBusSocket
{
    /// <summary>
    /// The NetMQ publisher socket for sending messages.
    /// </summary>
    private readonly PublisherSocket _publisher;

    /// <summary>
    /// The NetMQ subscriber socket for receiving messages.
    /// </summary>
    private readonly SubscriberSocket _subscriber;

    /// <summary>
    /// Cancellation token source for the observable stream.
    /// </summary>
    private readonly CancellationTokenSource _observableCts = new();

    /// <summary>
    /// The observable stream of incoming messages.
    /// </summary>
    private readonly IObservable<string> _observable;

    /// <summary>
    /// The disposable box for managing cleanup of NetMQ resources.
    /// </summary>
    private readonly AsyncDisposableBox _disposable;

    /// <summary>
    /// The logger instance for this socket.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the NetMQMessageBusSocket class.
    /// </summary>
    /// <param name="cfg">The network configuration containing endpoints and serializer.</param>
    /// <param name="logger">The logger to use for this socket.</param>
    public NetMQMessageBusSocket(NetworkConfiguration cfg, ILogger logger)
    {
        _disposable = Disposable.AsyncBox(logger);
        _disposable += _publisher = new PublisherSocket();
        _publisher.Connect(cfg.Endpoints.PubEndpoint);

        _disposable += _subscriber = new SubscriberSocket();
        _subscriber.Connect(cfg.Endpoints.SubEndpoint);
        _subscriber.SubscribeToAnyTopic();

        _observable = ObservableExt
            .StaticAsyncInstance<string>(CreateObservableAsync, _observableCts.Token, logger)
            .TrackCompletion(logger);
        _logger = logger;
    }

    /// <summary>
    /// Sends a string message through the socket.
    /// </summary>
    /// <param name="message">The message string to send.</param>
    /// <returns>An observable that completes when the message is sent.</returns>
    public IObservable<Unit> Send(string message) =>
        Observable.FromAsync(() =>
        {
            lock (_publisher)
                _publisher.SendFrame(message);

            return Task.CompletedTask;
        });

    /// <summary>
    /// Subscribes an observer to the message stream.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <returns>A disposable that can be used to unsubscribe.</returns>
    public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

    /// <summary>
    /// Creates the observable stream that receives messages from the NetMQ subscriber.
    /// </summary>
    /// <param name="ctx">The observer context for the stream.</param>
    /// <returns>A task that returns a cleanup function.</returns>
    private Task<Func<Task>> CreateObservableAsync(ObserverContext<string> ctx)
    {
        try
        {
            while (!ctx.Ct.IsCancellationRequested)
            {
                // TODO: use async method with receiving single message
                var msg = _subscriber.ReceiveMultipartStrings(Encoding.UTF8);
                if (msg.Count == 0)
                    continue;

                var message = string.Join(string.Empty, msg);
                ctx.OnNext(message);
            }
        }
        // token was canceled
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            ctx.OnError(e);
        }

        return Task.FromResult<Func<Task>>(() => Task.CompletedTask);
    }

    #region IDisposable support

    /// <summary>
    /// Asynchronously disposes the socket and its NetMQ resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await _observableCts.CancelAsync();
        await _observable.WhenCompletedAsync(_logger);

        await _disposable.DisposeAsync();
    }

    #endregion
}
