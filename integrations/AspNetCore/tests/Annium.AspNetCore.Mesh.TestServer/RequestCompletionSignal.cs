using System.Threading;
using System.Threading.Tasks;

namespace Annium.AspNetCore.Mesh.TestServer;

/// <summary>
/// Test-only signal fired by the wrapping middleware registered in <c>Program</c> once the downstream
/// middleware chain (in particular <c>Annium.AspNetCore.Mesh.Internal.Middleware.WebSocketsMiddleware</c>)
/// has fully returned for a given request. Because the wrapping middleware simply <c>await</c>s its
/// <c>next</c> delegate before firing this signal, observing it is a genuine happens-after relationship with
/// respect to everything the downstream middleware did while handling the request — including any (real or
/// mutated) call it made to <c>ICoordinator.HandleAsync</c> — rather than a race against a fixed wait window.
/// Hosts that need this guarantee register an instance of this class in their DI container; hosts that don't
/// care about it simply leave it unregistered, in which case the wrapping middleware is a no-op pass-through.
/// </summary>
public sealed class RequestCompletionSignal
{
    /// <summary>
    /// Completes once <see cref="SignalCompleted" /> has been invoked.
    /// </summary>
    public Task Completed => _completed.Task;

    /// <summary>
    /// Signals that the wrapped downstream middleware chain has fully returned.
    /// </summary>
    private readonly TaskCompletionSource _completed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Marks <see cref="Completed" /> as finished.
    /// </summary>
    public void SignalCompleted() => _completed.TrySetResult();
}
