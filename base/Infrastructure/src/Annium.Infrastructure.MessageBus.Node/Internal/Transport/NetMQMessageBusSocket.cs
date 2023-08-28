using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NetMQ;
using NetMQ.Sockets;

namespace Annium.Infrastructure.MessageBus.Node.Internal.Transport;

// ReSharper disable once InconsistentNaming
internal class NetMQMessageBusSocket : IMessageBusSocket
{
    private readonly PublisherSocket _publisher;
    private readonly SubscriberSocket _subscriber;
    private readonly CancellationTokenSource _observableCts = new();
    private readonly IObservable<string> _observable;
    private readonly AsyncDisposableBox _disposable;

    public NetMQMessageBusSocket(
        NetworkConfiguration cfg,
        ILogger logger
    )
    {
        _disposable = Disposable.AsyncBox(logger);
        _disposable += _publisher = new PublisherSocket();
        _publisher.Connect(cfg.Endpoints.PubEndpoint);

        _disposable += _subscriber = new SubscriberSocket();
        _subscriber.Connect(cfg.Endpoints.SubEndpoint);
        _subscriber.SubscribeToAnyTopic();

        _observable = ObservableExt.StaticAsyncInstance<string>(CreateObservable, _observableCts.Token).TrackCompletion();
    }

    public IObservable<Unit> Send(string message)
        => Observable.FromAsync(() =>
        {
            lock (_publisher)
                _publisher.SendFrame(message);

            return Task.CompletedTask;
        });

    public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

    private Task<Func<Task>> CreateObservable(ObserverContext<string> ctx)
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
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            ctx.OnError(e);
        }

        return Task.FromResult<Func<Task>>(() => Task.CompletedTask);
    }

    #region IDisposable support

    public async ValueTask DisposeAsync()
    {
        _observableCts.Cancel();
        await _observable.WhenCompleted();

        await _disposable.DisposeAsync();
    }

    #endregion
}