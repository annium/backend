using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

/// <summary>
/// Represents a client-side connection in the in-memory transport implementation
/// </summary>
internal class ClientConnection : IClientConnection
{
    /// <summary>
    /// Event raised when the connection is established
    /// </summary>
    public event Action OnConnected = delegate { };

    /// <summary>
    /// Event raised when the connection is closed
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs on the connection
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Event raised when data is received from the server
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying communication channel for this connection
    /// </summary>
    private readonly Channel _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnection"/> class
    /// </summary>
    /// <param name="channel">The underlying channel for communication</param>
    public ClientConnection(Channel channel)
    {
        _channel = channel;
        _channel.OnConnected += () => OnConnected();
        _channel.OnDisconnected += side =>
            OnDisconnected(
                side is CloseSide.Client ? ConnectionCloseStatus.ClosedLocal : ConnectionCloseStatus.ClosedRemote
            );
        _channel.OnClientReceived += data => OnReceived(data);
    }

    /// <summary>
    /// Establishes the connection to the server
    /// </summary>
    public void Connect()
    {
        _channel.Connect();
    }

    /// <summary>
    /// Closes the connection from the client side
    /// </summary>
    public void Disconnect()
    {
        _channel.Disconnect(CloseSide.Client);
    }

    /// <summary>
    /// Sends data to the server
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return _channel.SendToServerAsync(data, ct);
    }
}
