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
