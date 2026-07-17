using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.AspNetCore.Mesh.Tests.TestDoubles;

/// <summary>
/// Test double for <see cref="IServerConnectionFactory{TContext}" /> that records the <see cref="WebSocket" />
/// it was invoked with and always succeeds, returning a fixed <see cref="FakeServerConnection" /> instance so
/// tests can assert the exact same connection instance is later handed to the coordinator.
/// </summary>
internal sealed class RecordingConnectionFactory : IServerConnectionFactory<WebSocket>
{
    /// <summary>
    /// Completes with the <see cref="WebSocket" /> passed to <see cref="CreateAsync" />, once it has been called.
    /// </summary>
    public Task<WebSocket> Created => _created.Task;

    /// <summary>
    /// The fixed connection instance <see cref="CreateAsync" /> always returns.
    /// </summary>
    public IServerConnection Connection { get; } = new FakeServerConnection();

    /// <summary>
    /// Signals the <see cref="WebSocket" /> passed to <see cref="CreateAsync" />.
    /// </summary>
    private readonly TaskCompletionSource<WebSocket> _created = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Records <paramref name="context" /> on <see cref="Created" /> and always succeeds, returning the fixed
    /// <see cref="Connection" /> instance.
    /// </summary>
    /// <param name="context">The <see cref="WebSocket" /> the connection is created from.</param>
    /// <returns>A completed <see cref="Task{TResult}" /> holding <see cref="Connection" />.</returns>
    public Task<IServerConnection> CreateAsync(WebSocket context)
    {
        _created.TrySetResult(context);

        return Task.FromResult(Connection);
    }
}
