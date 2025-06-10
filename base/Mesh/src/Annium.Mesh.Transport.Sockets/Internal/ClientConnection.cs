using System;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

/// <summary>
/// Socket-based implementation of a client connection for mesh transport.
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
    /// The underlying client socket for network communication
    /// </summary>
    private readonly IClientSocket _socket;

    /// <summary>
    /// The URI to connect to
    /// </summary>
    private readonly Uri _uri;

    /// <summary>
    /// SSL client authentication options for secure connections
    /// </summary>
    private readonly SslClientAuthenticationOptions? _authOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnection"/> class.
    /// </summary>
    /// <param name="socket">The client socket to use for communication.</param>
    /// <param name="uri">The URI to connect to.</param>
    /// <param name="authOptions">The SSL client authentication options, if any.</param>
    /// <param name="logger">The logger instance.</param>
    public ClientConnection(IClientSocket socket, Uri uri, SslClientAuthenticationOptions? authOptions, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
        _socket.OnConnected += HandleConnected;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnReceived += HandleReceived;
        _uri = uri;
        _authOptions = authOptions;
    }

    /// <summary>
    /// Establishes a connection to the configured remote endpoint
    /// </summary>
    public void Connect()
    {
        this.Trace("start");

        _socket.Connect(_uri, _authOptions);

        this.Trace("done");
    }

    /// <summary>
    /// Closes the connection to the remote endpoint
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Sends data asynchronously through the connection
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
    /// Handles the connected event from the underlying socket
    /// </summary>
    private void HandleConnected()
    {
        this.Trace("trigger connected");

        OnConnected();

        this.Trace("done");
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
