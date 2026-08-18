using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Socket-based implementation of a managed connection for mesh transport.
/// </summary>
internal sealed class ManagedConnection : IManagedConnection, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this connection
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event triggered when the connection is disconnected
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event triggered when an error occurs on the connection
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Event is invoked, when message is received.
    /// Message must be processed synchronously due to possible buffer overwriting in implementing transports
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying server socket for network communication
    /// </summary>
    private readonly IServerSocket _socket;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedConnection"/> class.
    /// </summary>
    /// <param name="socket">The server socket to manage.</param>
    /// <param name="logger">The logger instance.</param>
    public ManagedConnection(IServerSocket socket, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnReceived += HandleReceived;
    }

    /// <summary>
    /// Closes the managed connection
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Sends data asynchronously through the managed connection
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
