using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal sealed class ManagedConnection : IManagedConnection, ILogSubject
{
    public ILogger Logger { get; }
    public Guid Id { get; } = Guid.NewGuid();
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private readonly IServerSocket _socket;

    public ManagedConnection(
        IServerSocket socket,
        ILogger logger
    )
    {
        Logger = logger;
        _socket = socket;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnReceived += HandleReceived;
    }

    public void Disconnect()
    {
        this.Trace("start");

        _socket.Disconnect();

        this.Trace("done");
    }

    public async ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("start");

        var status = await _socket.SendAsync(data, ct);

        this.Trace("done");

        return ConnectionSendStatusMap.Map(status);
    }

    private void HandleDisconnected(SocketCloseStatus status)
    {
        var mappedStatus = ConnectionCloseStatusMap.Map(status);
        this.Trace("trigger disconnected with {status}", mappedStatus);
        OnDisconnected(mappedStatus);
    }

    private void HandleError(Exception exception)
    {
        this.Trace("trigger error {exception}", exception);
        OnError(exception);
    }

    private void HandleReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger received");
        OnReceived(data);
    }
}