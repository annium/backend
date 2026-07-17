using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.AspNetCore.Mesh.Tests.TestDoubles;

/// <summary>
/// Test double for <see cref="IServerConnectionFactory{TContext}" /> that always throws, standing in for a
/// connection-factory failure occurring after the WebSocket handshake has already been accepted. Records that
/// it was invoked (via <see cref="Invoked" />) immediately before throwing, so tests can deterministically await
/// the failure path having run rather than racing a bounded timer against it.
/// </summary>
internal sealed class ThrowingConnectionFactory : IServerConnectionFactory<WebSocket>
{
    /// <summary>
    /// The exception message thrown by <see cref="CreateAsync" />.
    /// </summary>
    public const string Message = "connection factory failure";

    /// <summary>
    /// Completes once <see cref="CreateAsync" /> has been invoked, immediately before it throws.
    /// </summary>
    public Task Invoked => _invoked.Task;

    /// <summary>
    /// Signals that <see cref="CreateAsync" /> was invoked.
    /// </summary>
    private readonly TaskCompletionSource _invoked = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Signals <see cref="Invoked" /> and always throws, standing in for a connection-factory failure occurring
    /// after the WebSocket handshake has already been accepted.
    /// </summary>
    /// <param name="context">The <see cref="WebSocket" /> the connection would be created from.</param>
    /// <returns>Never returns: this method always throws <see cref="InvalidOperationException" />.</returns>
    public Task<IServerConnection> CreateAsync(WebSocket context)
    {
        _invoked.TrySetResult();
        throw new InvalidOperationException(Message);
    }
}
