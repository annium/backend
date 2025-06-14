// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Provides context for pushing messages to specific connected clients without expecting a response.
/// </summary>
/// <typeparam name="TMessage">The type of message to push.</typeparam>
public interface IPushContext<TMessage>
{
    /// <summary>
    /// Sends the specified message to the target client.
    /// </summary>
    /// <param name="message">The message to send.</param>
    void Send(TMessage message);
}
