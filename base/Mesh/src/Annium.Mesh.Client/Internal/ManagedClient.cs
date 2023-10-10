using System;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Client.Internal;

internal class ManagedClient : ClientBase, IManagedClient
{
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IManagedConnection _connection;

    public ManagedClient(
        IManagedConnection connection,
        ITimeProvider timeProvider,
        ISerializer<ReadOnlyMemory<byte>> serializer,
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

    protected override ValueTask HandleDisposeAsync()
    {
        this.Trace("start");

        this.Trace("disconnect connection");
        _connection.Disconnect();

        this.Trace("done");

        return ValueTask.CompletedTask;
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