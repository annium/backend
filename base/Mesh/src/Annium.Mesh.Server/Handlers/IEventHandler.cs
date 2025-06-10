using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

/// <summary>
/// Defines a handler for processing events received from mesh clients.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle.</typeparam>
public interface IEventHandler<TEvent>
{
    /// <summary>
    /// Handles the received event asynchronously.
    /// </summary>
    /// <param name="request">The request context containing the event data.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous event handling operation.</returns>
    Task HandleAsync(IRequestContext<TEvent> request, CancellationToken ct);
}
