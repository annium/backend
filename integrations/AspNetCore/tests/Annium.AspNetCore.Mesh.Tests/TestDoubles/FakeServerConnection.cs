using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.AspNetCore.Mesh.Tests.TestDoubles;

/// <summary>
/// Minimal <see cref="IServerConnection" /> stub returned by <see cref="RecordingConnectionFactory" />.
/// None of its members are exercised by the mesh WebSockets middleware directly — it only needs to exist
/// and be passed through to the coordinator's connection-handling method — so this type intentionally does
/// nothing beyond satisfying the interface.
/// </summary>
internal sealed class FakeServerConnection : IServerConnection
{
    /// <summary>
    /// Event triggered when the connection is disconnected
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected
    {
        add { }
        remove { }
    }

    /// <summary>
    /// Event triggered when an error occurs on the connection
    /// </summary>
    public event Action<Exception> OnError
    {
        add { }
        remove { }
    }

    /// <summary>
    /// Event is invoked, when message is received.
    /// Message must be processed synchronously due to possible buffer overwriting in implementing transports
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived
    {
        add { }
        remove { }
    }

    /// <summary>
    /// Does nothing: disconnection is not exercised by this suite.
    /// </summary>
    public void Disconnect() { }

    /// <summary>
    /// Does nothing and always reports success, since no test exercises actually sending data over this connection.
    /// </summary>
    /// <param name="data">The data that would be sent.</param>
    /// <param name="ct">A token to observe for cancellation (unused).</param>
    /// <returns>A completed <see cref="ValueTask{TResult}" /> holding <see cref="ConnectionSendStatus.Ok" />.</returns>
    public ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
        ValueTask.FromResult(ConnectionSendStatus.Ok);
}
