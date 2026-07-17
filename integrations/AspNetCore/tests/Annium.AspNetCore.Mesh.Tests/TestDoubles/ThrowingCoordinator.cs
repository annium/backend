using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.AspNetCore.Mesh.Tests.TestDoubles;

/// <summary>
/// Test double for <see cref="ICoordinator" /> that always throws from <see cref="HandleAsync" />, standing
/// in for a coordinator failure occurring after the connection has already been created. Records the exact
/// connection instance it was invoked with (via <see cref="Invoked" />) before throwing, so tests can prove
/// the failure happened downstream of a real connection hand-off — with that same instance — rather than the
/// coordinator never having been reached at all.
/// </summary>
internal sealed class ThrowingCoordinator : ICoordinator
{
    /// <summary>
    /// The exception message thrown by <see cref="HandleAsync" />.
    /// </summary>
    public const string Message = "coordinator failure";

    /// <summary>
    /// Completes with the connection passed to <see cref="HandleAsync" />, once it has been called, immediately
    /// before it throws.
    /// </summary>
    public Task<IServerConnection> Invoked => _invoked.Task;

    /// <summary>
    /// Signals the connection passed to <see cref="HandleAsync" />.
    /// </summary>
    private readonly TaskCompletionSource<IServerConnection> _invoked = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    /// <summary>
    /// Signals <see cref="Invoked" /> with <paramref name="connection" /> and always throws, standing in for a
    /// coordinator failure occurring after the connection has already been created.
    /// </summary>
    /// <param name="connection">The connection that was handed off before the failure.</param>
    /// <returns>Never returns: this method always throws <see cref="InvalidOperationException" />.</returns>
    public Task HandleAsync(IServerConnection connection)
    {
        _invoked.TrySetResult(connection);
        throw new InvalidOperationException(Message);
    }

    /// <summary>
    /// Does nothing: disposal is not exercised by this suite.
    /// </summary>
    public void Dispose() { }
}
