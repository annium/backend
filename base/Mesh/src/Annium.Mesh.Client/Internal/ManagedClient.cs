using System;
using Annium.Logging;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

internal class ManagedClient : ClientBase, IManagedClient
{
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IManagedConnection _connection;

    public ManagedClient(
        IManagedConnection connection,
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
        _connection.OnDisconnected += HandleDisconnected;
        _connection.OnError += HandleError;
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
        OnConnected();
    }

    private void HandleDisconnected(ConnectionCloseStatus status)
    {
        OnDisconnected(status);
    }

    private void HandleError(Exception exception)
    {
        OnError(exception);
    }
}