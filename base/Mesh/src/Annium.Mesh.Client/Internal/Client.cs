using System;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

internal class Client : ClientBase, IClient
{
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IClientConnection _connection;
    private readonly object _locker = new();
    private bool _isConnected;
    private bool _isConnectionReady;

    public Client(
        IClientConnection connection,
        ITimeProvider timeProvider,
        ISerializer serializer,
        IClientConfiguration configuration,
        ILogger logger
    ) : base(
        connection,
        timeProvider,
        serializer,
        configuration,
        logger
    )
    {
        _connection = connection;
        _connection.OnConnected += HandleConnected;
        _connection.OnDisconnected += HandleDisconnected;
        _connection.OnError += HandleError;
    }

    public void Connect()
    {
        this.Trace("start");
        _connection.Connect();
        this.Trace("done");
    }

    public void Disconnect()
    {
        this.Trace("start");
        _connection.Disconnect();
        this.Trace("done");
    }

    protected override void HandleDispose()
    {
        this.Trace("disconnect connection");
        _connection.Disconnect();
    }

    protected override void HandleConnectionReady()
    {
        lock (_locker)
        {
            // if not connected - don't set flag
            if (!_isConnected)
                return;

            // set connection ready flag
            _isConnectionReady = true;
        }

        // invoke event outside of lock
        OnConnected();
    }

    private void HandleConnected()
    {
        lock (_locker)
        {
            // set connected flag
            _isConnected = true;

            // if not connected - don't set flag
            if (!_isConnectionReady)
                return;
        }

        // invoke event outside of lock
        OnConnected();
    }

    private void HandleDisconnected(ConnectionCloseStatus status)
    {
        lock (_locker)
        {
            // set connected flag
            _isConnected = false;

            // set connection ready flag
            _isConnectionReady = false;
        }

        // invoke event outside of lock
        OnDisconnected(status);
    }

    private void HandleError(Exception exception)
    {
        lock (_locker)
        {
            // set connected flag
            _isConnected = false;

            // set connection ready flag
            _isConnectionReady = false;
        }

        OnError(exception);
    }
}