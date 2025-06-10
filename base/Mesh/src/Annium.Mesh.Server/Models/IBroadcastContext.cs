namespace Annium.Mesh.Server.Models;

/// <summary>
/// Provides context for broadcasting messages to multiple connected clients in a mesh network.
/// </summary>
/// <typeparam name="TMessage">The type of message to broadcast.</typeparam>
public interface IBroadcastContext<TMessage>
    where TMessage : notnull
{
    /// <summary>
    /// Sends the specified message to all connected clients.
    /// </summary>
    /// <param name="message">The message to broadcast.</param>
    void Send(TMessage message);
}
