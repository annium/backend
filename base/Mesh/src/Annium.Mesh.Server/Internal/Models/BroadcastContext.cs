using System;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Models;

/// <summary>
/// Provides a context for broadcasting messages to connected clients.
/// </summary>
/// <typeparam name="TMessage">The type of messages to broadcast.</typeparam>
internal class BroadcastContext<TMessage> : IBroadcastContext<TMessage>
    where TMessage : notnull
{
    /// <summary>
    /// The action used to send messages to clients.
    /// </summary>
    private readonly Action<object> _send;

    /// <summary>
    /// Initializes a new instance of the <see cref="BroadcastContext{TMessage}"/> class.
    /// </summary>
    /// <param name="send">The action to invoke when sending messages to clients.</param>
    public BroadcastContext(Action<object> send)
    {
        _send = send;
    }

    /// <summary>
    /// Sends a message to all connected clients.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    public void Send(TMessage message) => _send(message);
}
