using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal sealed class ClientConnection : IClientConnection, ILogSubject
{
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    public ILogger Logger { get; }
    private readonly IClientSocket _socket;
    private readonly IPEndPoint _endpoint;
    private readonly SslClientAuthenticationOptions? _authOptions;

    public ClientConnection(
        IClientSocket socket,
        IPEndPoint endpoint,
        SslClientAuthenticationOptions? authOptions,
        ILogger logger
    )
    {
        Logger = logger;
        _socket = socket;
        _socket.OnConnected += HandleConnected;
        _socket.OnDisconnected += HandleDisconnected;
        _socket.OnError += HandleError;
        _socket.OnReceived += HandleReceived;
        _endpoint = endpoint;
        _authOptions = authOptions;
    }

    public void Connect()
    {
        this.Trace("start");

        _socket.Connect(_endpoint, _authOptions);

        this.Trace("done");
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

    private void HandleConnected()
    {
        this.Trace("trigger connected");
        OnConnected();
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