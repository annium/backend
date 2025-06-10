using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Socket-based implementation of a server connection for mesh transport.
/// </summary>
internal sealed class ServerConnection : IServerConnection, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this connection
    /// </summary>
    public ILogger Logger { get; }

    /// <inheritdoc />
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <inheritdoc />
    public event Action<Exception> OnError = delegate { };

    /// <inheritdoc />
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying server socket for network communication
    /// </summary>
    private readonly IServerSocket _socket;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConnection"/> class.
    /// </summary>
    /// <param name="socket">The server socket to use for communication.</param>
    /// <param name="logger">The logger instance.</param>
    public ServerConnection(IServerSocket socket, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnReceived += HandleReceived;
    }

    /// <summary>
    /// Closes the server connection
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Sends data asynchronously through the server connection
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The connection send status indicating the result of the operation</returns>
    public async ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("start");

        var status = await _socket.SendAsync(data, ct);

        this.Trace("done");

        return ConnectionSendStatusMap.Map(status);
    }

    /// <summary>
    /// Handles the disconnected event from the underlying socket
    /// </summary>
    /// <param name="status">The socket close status</param>
    private void HandleDisconnected(SocketCloseStatus status)
    {
        var mappedStatus = ConnectionCloseStatusMap.Map(status);

        this.Trace("trigger disconnected with {status}", mappedStatus);

        OnDisconnected(mappedStatus);

        this.Trace("done");
    }

    /// <summary>
    /// Handles error events from the underlying socket
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    private void HandleError(Exception exception)
    {
        this.Trace("trigger error {exception}", exception);

        OnError(exception);

        this.Trace("done");
    }

    /// <summary>
    /// Handles data received events from the underlying socket
    /// </summary>
    /// <param name="data">The received data</param>
    private void HandleReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger received");

        OnReceived(data);

        this.Trace("done");
    }
}
