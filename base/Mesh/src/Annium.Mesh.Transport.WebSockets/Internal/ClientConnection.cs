using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Mesh.Transport.WebSockets.Internal;

/// <summary>
/// WebSocket-based implementation of a client connection for mesh transport.
/// </summary>
internal sealed class ClientConnection : IClientConnection, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this connection
    /// </summary>
    public ILogger Logger { get; }

    /// <inheritdoc />
    public event Action OnConnected = delegate { };

    /// <inheritdoc />
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <inheritdoc />
    public event Action<Exception> OnError = delegate { };

    /// <inheritdoc />
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying client WebSocket for network communication
    /// </summary>
    private readonly IClientWebSocket _socket;

    /// <summary>
    /// The URI to connect to
    /// </summary>
    private readonly Uri _uri;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnection"/> class.
    /// </summary>
    /// <param name="socket">The client WebSocket to use for communication.</param>
    /// <param name="uri">The URI to connect to.</param>
    /// <param name="logger">The logger instance.</param>
    public ClientConnection(IClientWebSocket socket, Uri uri, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
        _socket.OnConnected += HandleConnected;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnBinaryReceived += HandleReceived;
        _uri = uri;
    }

    /// <summary>
    /// Establishes a WebSocket connection to the configured remote endpoint
    /// </summary>
    public void Connect()
    {
        this.Trace("start");

        _socket.Connect(_uri);

        this.Trace("done");
    }

    /// <summary>
    /// Closes the WebSocket connection to the remote endpoint
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Sends data asynchronously through the WebSocket connection
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
    /// Handles the connected event from the underlying WebSocket
    /// </summary>
    private void HandleConnected()
    {
        this.Trace("trigger connected");

        OnConnected();

        this.Trace("done");
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
