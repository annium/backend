using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using NetMQ;
using NetMQ.Sockets;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class MessageBusSocket : IMessageBusSocket
    {
        private readonly PublisherSocket _publisher;
        private readonly SubscriberSocket _subscriber;
        private readonly IObservable<string> _observable;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        public MessageBusSocket(
            Configuration cfg
        )
        {
            _disposable += _publisher = new PublisherSocket();
            _publisher.Connect(cfg.Endpoints.PubEndpoint);

            _disposable += _subscriber = new SubscriberSocket();
            _subscriber.Connect(cfg.Endpoints.SubEndpoint);
            _subscriber.SubscribeToAnyTopic();

            _observable = Observable.Create<string>(CreateObservable).Publish().RefCount();
        }

        public IObservable<Unit> Send(string message)
            => Observable.FromAsync(() =>
            {
                lock (_publisher)
                    _publisher.SendFrame(message);

                return Task.CompletedTask;
            });

        public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

        private Task CreateObservable(
            IObserver<string> observer,
            CancellationToken ct
        ) => Task.Run(() =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var msg = _subscriber.ReceiveMultipartStrings(Encoding.UTF8);
                    if (msg.Count == 0)
                        continue;

                    var message = string.Join(string.Empty, msg);
                    observer.OnNext(message);
                }
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                observer.OnCompleted();
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
        }, ct);

        #region IDisposable support

        public async ValueTask DisposeAsync()
        {
            await _disposable.DisposeAsync();
        }

        #endregion
    }
}