using System;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Mesh.Transport.Abstractions;
using Annium.Net.Sockets;

namespace Annium.Mesh.Transport.Sockets.Internal;

internal sealed class ClientConnection : IClientConnection, ILogSubject
{
    public ILogger Logger { get; }
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private readonly IClientSocket _socket;
    private readonly Uri _uri;
    private readonly SslClientAuthenticationOptions? _authOptions;

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

    public void Connect()
    {
        this.Trace("start");

        _socket.Connect(_uri, _authOptions);

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

        this.Trace("done");
    }

    private void HandleDisconnected(SocketCloseStatus status)
    {
        var mappedStatus = ConnectionCloseStatusMap.Map(status);

        this.Trace("trigger disconnected with {status}", mappedStatus);

        OnDisconnected(mappedStatus);

        this.Trace("done");
    }

    private void HandleError(Exception exception)
    {
        this.Trace("trigger error {exception}", exception);

        OnError(exception);

        this.Trace("done");
    }

    private void HandleReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger received");

        OnReceived(data);

        this.Trace("done");
    }
}
