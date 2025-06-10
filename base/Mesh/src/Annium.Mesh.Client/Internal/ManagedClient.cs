using System;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

/// <summary>
/// Internal implementation of a managed mesh client for externally managed connections
/// </summary>
internal class ManagedClient : ClientBase, IManagedClient
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
    /// The underlying managed connection instance
    /// </summary>
    private readonly IManagedConnection _connection;

    /// <summary>
    /// Initializes a new instance of the ManagedClient class
    /// </summary>
    /// <param name="connection">The managed connection to use</param>
    /// <param name="timeProvider">The time provider for timeout operations</param>
    /// <param name="serializer">The serializer for message data</param>
    /// <param name="configuration">The client configuration</param>
    /// <param name="logger">The logger for diagnostics</param>
    public ManagedClient(
        IManagedConnection connection,
        ITimeProvider timeProvider,
        ISerializer serializer,
        IClientConfiguration configuration,
        ILogger logger
    )
        : base(connection, timeProvider, serializer, configuration, logger)
    {
        _connection = connection;
        _connection.OnDisconnected += HandleDisconnected;
        _connection.OnError += HandleError;
    }

    /// <summary>
    /// Closes the connection to the server
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        _connection.Disconnect();

        this.Trace("done");
    }

    /// <summary>
    /// Handles disposal of managed client resources
    /// </summary>
    protected override void HandleDispose()
    {
        this.Trace("start");

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

        this.Trace("fire OnError: {error}", exception);
        OnError(exception);

        this.Trace("done");
    }
}
