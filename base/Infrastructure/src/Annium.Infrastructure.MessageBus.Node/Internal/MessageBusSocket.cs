using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly EndpointsConfiguration _cfg;
        private readonly PublisherSocket _publisher;
        private readonly Dictionary<string, IObservable<string>> _observables = new();
        private readonly IAsyncDisposableBox _disposable = Disposable.AsyncBox();

        public MessageBusSocket(
            Configuration cfg
        )
        {
            _cfg = cfg.Endpoints;
            _disposable.Add(_publisher = new PublisherSocket());
            _publisher.Connect(_cfg.PubEndpoint);
        }

        public IObservable<Unit> Send(string topic, string message)
            => Observable.FromAsync(() =>
            {
                lock (_publisher)
                    _publisher.SendFrame($"{topic}{message}");

                return Task.CompletedTask;
            });

        public IObservable<string> Listen(string topic)
        {
            lock (_observables)
            {
                if (_observables.TryGetValue(topic, out var observable))
                    return observable;

                var subscriber = new SubscriberSocket();
                _disposable.Add(subscriber);
                subscriber.Connect(_cfg.SubEndpoint);
                if (string.IsNullOrWhiteSpace(topic))
                    subscriber.SubscribeToAnyTopic();
                else
                    subscriber.Subscribe(topic);

                return _observables[topic] = Observable.Create(CreateObservableFactory(subscriber, topic)).Publish().RefCount();
            }
        }

        private Func<IObserver<string>, CancellationToken, Task> CreateObservableFactory(
            SubscriberSocket subscriber,
            string topic
        ) => (observer, token) => Task.Run(() =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var msg = subscriber.ReceiveMultipartStrings(Encoding.UTF8);

                    if (msg.Count == 0)
                        continue;

                    var message = topic.Length > 0
                        ? string.Join(string.Empty, msg.Skip(1).Prepend(msg[0][topic.Length..]))
                        : string.Join(string.Empty, msg);

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
        }, token);

        #region IDisposable support

        public async ValueTask DisposeAsync()
        {
            await _disposable.DisposeAsync();
        }

        #endregion
    }
}
