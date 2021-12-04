using System;
using System.Reactive;
using System.Reactive.Linq;
using Annium.Infrastructure.MessageBus.Node.Internal.Transport;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

internal class MessageBusNode : IMessageBusNode
{
    private readonly IMessageBusSocket _socket;
    private readonly ISerializer<string> _serializer;
    private readonly IObservable<Message> _observable;

    public MessageBusNode(
        IConfiguration cfg,
        IMessageBusSocket socket
    )
    {
        _socket = socket;
        _serializer = cfg.Serializer;
        _observable = _socket.Select(x => _serializer.Deserialize<Message>(x)).Publish().RefCount();
    }

    public IObservable<Unit> Send<T>(T data)
        where T : notnull
    {
        var message = Activator.CreateInstance(typeof(Message<>).MakeGenericType(data.GetType()), data)!;

        return _socket.Send(_serializer.Serialize(message));
    }

    public IObservable<object> Listen() => Listen<object>();

    public IObservable<T> Listen<T>() => _observable
        .OfType<IMessage<T>>()
        .Select(x => x.Content);

    public IDisposable Subscribe(IObserver<string> observer) => _socket.Subscribe(observer);
}