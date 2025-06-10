using System;
using System.Reactive;

namespace Annium.Infrastructure.MessageBus.Node;

/// <summary>
/// Defines a low-level socket interface for MessageBus communication, handling raw string message transmission.
/// </summary>
public interface IMessageBusSocket : IObservable<string>, IAsyncDisposable
{
    /// <summary>
    /// Sends a string message through the socket.
    /// </summary>
    /// <param name="message">The message string to send.</param>
    /// <returns>An observable that completes when the message is sent.</returns>
    IObservable<Unit> Send(string message);
}
