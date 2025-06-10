using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

/// <summary>
/// WebSocket-based implementation of a managed connection for mesh transport.
/// </summary>
internal sealed class ManagedConnection : IManagedConnection, ILogSubject
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
    /// The underlying server WebSocket for network communication
    /// </summary>
    private readonly IServerWebSocket _socket;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedConnection"/> class.
    /// </summary>
    /// <param name="socket">The server WebSocket to manage.</param>
    /// <param name="logger">The logger instance.</param>
    public ManagedConnection(IServerWebSocket socket, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnBinaryReceived += HandleReceived;
    }

    /// <summary>
    /// Closes the managed WebSocket connection
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Sends data asynchronously through the managed WebSocket connection
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The connection send status indicating the result of the operation</returns>
    public async ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("start");

        var status = await _socket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return ConnectionSendStatusMap.Map(status);
    }

    /// <summary>
    /// Handles the disconnected event from the underlying WebSocket
    /// </summary>
    /// <param name="status">The WebSocket close status</param>
    private void HandleDisconnected(WebSocketCloseStatus status)
    {
        var mappedStatus = ConnectionCloseStatusMap.Map(status);

        this.Trace("trigger disconnected with {status}", mappedStatus);

        OnDisconnected(mappedStatus);

        this.Trace("done");
    }

    /// <summary>
    /// Handles error events from the underlying WebSocket
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    private void HandleError(Exception exception)
    {
        this.Trace("trigger error {exception}", exception);

        OnError(exception);

        this.Trace("done");
    }

    /// <summary>
    /// Handles data received events from the underlying WebSocket
    /// </summary>
    /// <param name="data">The received data</param>
    private void HandleReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger received");

        OnReceived(data);

        this.Trace("done");
    }
}
