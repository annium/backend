using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class TestMessageBusSocket : IMessageBusSocket
    {
        private readonly object _lock = new();
        private readonly ManualResetEventSlim _gate = new(false);
        private readonly Dictionary<string, Queue<string>> _messages = new();
        private bool _isDisposed;

        public IObservable<Unit> Send(string topic, string message)
        {
            if (topic == null)
                throw new ArgumentNullException(nameof(topic));

            lock (_lock)
            {
                // add message
                if (!_messages.TryGetValue(topic, out var box))
                    box = _messages[topic] = new Queue<string>();
                box.Enqueue(message);

                // open gait to let messages be read
                _gate.Set();
            }

            return Observable.Return(Unit.Default);
        }

        public IObservable<string> Listen(string topic) => Observable.Create<string>(observer =>
        {
            Func<IEnumerable<string>> getMessages = string.IsNullOrWhiteSpace(topic) ? GetAllMessages : GetTopicMessages(topic);
            while (!_isDisposed)
            {
                _gate.Wait();
                foreach (var item in getMessages())
                    observer.OnNext(item);
                _gate.Reset();
            }

            return () => { };
        });

        public ValueTask DisposeAsync()
        {
            _isDisposed = true;

            return new ValueTask(Task.CompletedTask);
        }

        private IEnumerable<string> GetAllMessages()
        {
            foreach (var box in _messages.Values)
                while (box.Count > 0)
                    yield return box.Dequeue();
        }

        private Func<IEnumerable<string>> GetTopicMessages(string topic) => () =>
        {
            if (!_messages.TryGetValue(topic, out var box))
                return Array.Empty<string>();

            var messages = new List<string>();
            while (box.Count > 0)
                messages.Add(box.Dequeue());

            return messages;
        };
    }
}
