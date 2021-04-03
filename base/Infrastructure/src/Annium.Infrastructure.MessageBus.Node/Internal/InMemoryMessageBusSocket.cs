using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class InMemoryMessageBusSocket : IMessageBusSocket
    {
        private readonly IObservable<Message> _observable;
        private readonly ManualResetEventSlim _gate = new(false);
        private readonly Dictionary<string, Queue<string>> _messages = new();
        private readonly IDisposableBox _disposable = Disposable.Box();

        public InMemoryMessageBusSocket(
            InMemoryConfiguration cfg
        )
        {
            _observable = Observable.Create<Message>(CreateObservable).Publish().RefCount();
            if (cfg.MessageBox is not null)
                _disposable.Add(Listen(string.Empty).Subscribe(cfg.MessageBox.Add));
        }

        public IObservable<Unit> Send(string topic, string message)
        {
            if (topic is null)
                throw new ArgumentNullException(nameof(topic));

            lock (_messages)
            {
                // add message
                if (!_messages.TryGetValue(topic, out var box))
                    box = _messages[topic] = new Queue<string>();
                box.Enqueue(message);
            }

            // open gate to let messages be read
            _gate.Set();

            return Observable.Return(Unit.Default);
        }

        public IObservable<string> Listen(string topic) => _observable
            .Where(x => x.Topic == topic)
            .Select(x => x.Content);

        private Task CreateObservable(
            IObserver<Message> observer,
            CancellationToken ct
        ) => Task.Run(() =>
        {
            while (!ct.IsCancellationRequested)
            {
                _gate.Wait(ct);
                _gate.Reset();

                var messages = new List<Message>();
                lock (_messages)
                {
                    foreach (var (topic, box) in _messages)
                        while (box.Count > 0)
                            messages.Add(new Message(topic, box.Dequeue()));
                }

                foreach (var item in messages)
                    observer.OnNext(item);
            }
        }, ct);

        public ValueTask DisposeAsync()
        {
            return new(Task.CompletedTask);
        }

        private record Message(string Topic, string Content);
    }
}