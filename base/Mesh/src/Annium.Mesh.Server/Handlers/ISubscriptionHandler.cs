using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Annium.Mesh.Server;

/// <summary>
/// Defines a handler for managing subscriptions where clients can receive continuous updates after initial setup.
/// </summary>
/// <typeparam name="TInit">The type of initialization data for the subscription.</typeparam>
/// <typeparam name="TMessage">The type of messages sent to subscribers.</typeparam>
public interface ISubscriptionHandler<TInit, TMessage>
{
    /// <summary>
    /// Handles the subscription request and manages the ongoing subscription lifecycle.
    /// </summary>
    /// <param name="request">The subscription context containing initialization data and messaging capabilities.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous subscription handling operation.</returns>
    Task HandleAsync(ISubscriptionContext<TInit, TMessage> request, CancellationToken ct);
}
