using System;
using System.Reactive;
using System.Reactive.Linq;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal
{
    internal class MessageBusNode : IMessageBusNode
    {
        private readonly IMessageBusSocket _socket;
        private readonly ISerializer<string> _serializer;

        public MessageBusNode(
            IConfiguration cfg,
            IMessageBusSocket socket
        )
        {
            _socket = socket;
            _serializer = cfg.Serializer;
        }

        public IObservable<Unit> Send<T>(T data) => _socket.Send(string.Empty, _serializer.Serialize(data));

        public IObservable<Unit> Send<T>(string topic, T data) => _socket.Send(topic, _serializer.Serialize(data));

        public IObservable<string> Listen() => _socket.Listen(string.Empty);

        public IObservable<string> Listen(string topic) => _socket.Listen(topic);

        public IObservable<T> Listen<T>() => _socket.Listen(string.Empty).Select(_serializer.Deserialize<T>);

        public IObservable<T> Listen<T>(string topic) => _socket.Listen(topic).Select(_serializer.Deserialize<T>);
    }
}