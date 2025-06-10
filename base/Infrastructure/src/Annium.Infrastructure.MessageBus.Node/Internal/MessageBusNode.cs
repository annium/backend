using System;
using System.Reactive;
using System.Reactive.Linq;
using Annium.Infrastructure.MessageBus.Node.Internal.Transport;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.MessageBus.Node.Internal;

/// <summary>
/// Implementation of IMessageBusNode that provides reactive message sending and receiving capabilities.
/// </summary>
internal class MessageBusNode : IMessageBusNode
{
    /// <summary>
    /// The underlying socket used for message transmission.
    /// </summary>
    private readonly IMessageBusSocket _socket;

    /// <summary>
    /// The serializer used for message serialization and deserialization.
    /// </summary>
    private readonly ISerializer<string> _serializer;

    /// <summary>
    /// The observable stream of deserialized messages.
    /// </summary>
    private readonly IObservable<Message> _observable;

    /// <summary>
    /// Initializes a new instance of the MessageBusNode class.
    /// </summary>
    /// <param name="cfg">The configuration containing the serializer.</param>
    /// <param name="socket">The socket to use for message transmission.</param>
    public MessageBusNode(IConfiguration cfg, IMessageBusSocket socket)
    {
        _socket = socket;
        _serializer = cfg.Serializer;
        _observable = _socket.Select(x => _serializer.Deserialize<Message>(x)).Publish().RefCount();
    }

    /// <summary>
    /// Sends data to the MessageBus and returns an observable representing the send operation.
    /// </summary>
    /// <typeparam name="T">The type of data to send, must be non-null.</typeparam>
    /// <param name="data">The data to send through the MessageBus.</param>
    /// <returns>An observable that completes when the message is sent.</returns>
    public IObservable<Unit> Send<T>(T data)
        where T : notnull
    {
        var message = Activator.CreateInstance(typeof(Message<>).MakeGenericType(data.GetType()), data)!;

        return _socket.Send(_serializer.Serialize(message));
    }

    /// <summary>
    /// Listens for all incoming messages from the MessageBus.
    /// </summary>
    /// <returns>An observable stream of incoming message objects.</returns>
    public IObservable<object> Listen() => Listen<object>();

    /// <summary>
    /// Listens for incoming messages of a specific type from the MessageBus.
    /// </summary>
    /// <typeparam name="T">The type of messages to listen for.</typeparam>
    /// <returns>An observable stream of incoming messages of the specified type.</returns>
    public IObservable<T> Listen<T>() => _observable.OfType<IMessage<T>>().Select(x => x.Content);

    /// <summary>
    /// Subscribes an observer to the raw string message stream from the socket.
    /// </summary>
    /// <param name="observer">The observer to subscribe to the message stream.</param>
    /// <returns>A disposable that can be used to unsubscribe the observer.</returns>
    public IDisposable Subscribe(IObserver<string> observer) => _socket.Subscribe(observer);
}
