using System;
using System.Reactive;

namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Defines a MessageBus node that can send and receive messages through reactive streams.
/// </summary>
public interface IMessageBusNode : IObservable<string>
{
    /// <summary>
    /// Sends data to the MessageBus and returns an observable representing the send operation.
    /// </summary>
    /// <typeparam name="T">The type of data to send, must be non-null.</typeparam>
    /// <param name="data">The data to send through the MessageBus.</param>
    /// <returns>An observable that completes when the message is sent.</returns>
    IObservable<Unit> Send<T>(T data)
        where T : notnull;

    /// <summary>
    /// Listens for all incoming messages from the MessageBus.
    /// </summary>
    /// <returns>An observable stream of incoming message objects.</returns>
    IObservable<object> Listen();

    /// <summary>
    /// Listens for incoming messages of a specific type from the MessageBus.
    /// </summary>
    /// <typeparam name="T">The type of messages to listen for.</typeparam>
    /// <returns>An observable stream of incoming messages of the specified type.</returns>
    IObservable<T> Listen<T>();
}
