using System;
using System.Threading;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

/// <summary>
/// Internal implementation of a mesh client with connection management capabilities
/// </summary>
internal class Client : ClientBase, IClient
{
    /// <summary>
    /// Event fired when the client successfully connects to the server
    /// </summary>
    public event Action OnConnected = delegate { };

    /// <summary>
    /// Event fired when the client disconnects from the server
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event fired when an error occurs during client operations
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// The underlying client connection instance
    /// </summary>
    private readonly IClientConnection _connection;

    /// <summary>
    /// Lock object for thread-safe access to connection state
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Flag indicating whether the client is connected
    /// </summary>
    private bool _isConnected;

    /// <summary>
    /// Flag indicating whether the connection is ready for communication
    /// </summary>
    private bool _isConnectionReady;

    /// <summary>
    /// Initializes a new instance of the Client class
    /// </summary>
    /// <param name="connection">The client connection to use</param>
    /// <param name="timeProvider">The time provider for timeout operations</param>
    /// <param name="serializer">The serializer for message data</param>
    /// <param name="configuration">The client configuration</param>
    /// <param name="logger">The logger for diagnostics</param>
    public Client(
        IClientConnection connection,
        ITimeProvider timeProvider,
        ISerializer serializer,
        IClientConfiguration configuration,
        ILogger logger
    )
        : base(connection, timeProvider, serializer, configuration, logger)
    {
        this.Trace("start");
        _connection = connection;
        _connection.OnConnected += HandleConnected;
        _connection.OnDisconnected += HandleDisconnected;
        _connection.OnError += HandleError;
        this.Trace("done");
    }

    /// <summary>
    /// Establishes a connection to the server
    /// </summary>
    public void Connect()
    {
        this.Trace("start");

        lock (_locker)
        {
            _isConnected = true;
            _isConnectionReady = false;
        }

        _connection.Connect();

        this.Trace("done");
    }

    /// <summary>
    /// Closes the connection to the server
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        lock (_locker)
        {
            _isConnected = false;
            _isConnectionReady = false;
        }

        _connection.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Handles disposal of client resources
    /// </summary>
    protected override void HandleDispose()
    {
        this.Trace("start");

        lock (_locker)
        {
            _isConnected = false;
            _isConnectionReady = false;
        }

        this.Trace("disconnect connection");
        _connection.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the connection ready event from the underlying transport
    /// </summary>
    protected override void HandleConnectionReady()
    {
        this.Trace("start");

        lock (_locker)
        {
            // flag is set anyway (concurrency possible)
            _isConnectionReady = true;

            // if not connected - don't fire event
            if (!_isConnected)
            {
                this.Trace("not connected");

                return;
            }
        }

        // invoke event outside of lock
        this.Trace("fire OnConnected");
        OnConnected();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the connection event from the underlying transport
    /// </summary>
    private void HandleConnected()
    {
        lock (_locker)
        {
            // set connected flag
            _isConnected = true;

            // if not connection ready - don't fire event
            if (!_isConnectionReady)
            {
                this.Trace("not connection ready");

                return;
            }
        }

        // invoke event outside of lock
        this.Trace("fire OnConnected");
        OnConnected();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the disconnection event from the underlying transport
    /// </summary>
    /// <param name="status">The connection close status</param>
    private void HandleDisconnected(ConnectionCloseStatus status)
    {
        this.Trace("start");

        lock (_locker)
        {
            _isConnected = false;
            _isConnectionReady = false;
        }

        // invoke event outside of lock
        this.Trace("fire OnDisconnected: {status}", status);
        OnDisconnected(status);

        this.Trace("done");
    }

    /// <summary>
    /// Handles error events from the underlying transport
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    private void HandleError(Exception exception)
    {
        this.Trace("start");

        lock (_locker)
        {
            _isConnected = false;
            _isConnectionReady = false;
        }

        this.Trace("fire OnError: {error}", exception);
        OnError(exception);

        this.Trace("done");
    }
}
