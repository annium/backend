using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Defines a broadcaster that can send messages to multiple connected clients in a mesh network.
/// </summary>
/// <typeparam name="TMessage">The type of message to broadcast.</typeparam>
public interface IBroadcaster<TMessage>
    where TMessage : notnull
{
    /// <summary>
    /// Executes the broadcasting operation for the specified message.
    /// </summary>
    /// <param name="context">The broadcast context containing the message and connection information.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    Task RunAsync(IBroadcastContext<TMessage> context, CancellationToken ct);
}
