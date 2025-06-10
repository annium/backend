using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

/// <summary>
/// Defines a handler for push operations that send messages to connected clients without expecting a response.
/// </summary>
/// <typeparam name="TAction">The enum type representing the action for this push handler.</typeparam>
/// <typeparam name="TMessage">The type of message to push to clients.</typeparam>
public interface IPushHandler<TAction, TMessage> : IHandlerBase<TAction>
    where TAction : struct, Enum
    where TMessage : notnull
{
    /// <summary>
    /// Executes the push operation to send the message to connected clients.
    /// </summary>
    /// <param name="ctx">The push context containing the message and connection information.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous push operation.</returns>
    Task RunAsync(IPushContext<TMessage> ctx, CancellationToken ct);
}
