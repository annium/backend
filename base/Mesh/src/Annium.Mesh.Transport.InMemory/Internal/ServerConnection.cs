using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

/// <summary>
/// Represents a server-side connection in the in-memory transport implementation
/// </summary>
internal class ServerConnection : IServerConnection
{
    /// <summary>
    /// Event raised when the connection is closed
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs on the connection
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Event raised when data is received from the client
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying communication channel for this connection
    /// </summary>
    private readonly Channel _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConnection"/> class
    /// </summary>
    /// <param name="channel">The underlying channel for communication</param>
    public ServerConnection(Channel channel)
    {
        _channel = channel;
        _channel.OnDisconnected += side =>
            OnDisconnected(
                side is CloseSide.Client ? ConnectionCloseStatus.ClosedRemote : ConnectionCloseStatus.ClosedLocal
            );
        _channel.OnServerReceived += data => OnReceived(data);
    }

    /// <summary>
    /// Closes the connection from the server side
    /// </summary>
    public void Disconnect()
    {
        _channel.Disconnect(CloseSide.Server);
    }

    /// <summary>
    /// Sends data to the client
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return _channel.SendToClientAsync(data, ct);
    }
}
