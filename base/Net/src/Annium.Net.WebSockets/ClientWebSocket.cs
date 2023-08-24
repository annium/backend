using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

public class ClientWebSocket : IClientWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public event Action OnConnected = delegate { };
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IClientManagedWebSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Uri? _uri;
    private Status _status = Status.Disconnected;

    public ClientWebSocket(IConnectionMonitor monitor)
    {
        this.Trace("start monitor");
        _socket = new ClientManagedWebSocket();
        _socket.TextReceived += TextReceived;
        _socket.BinaryReceived += BinaryReceived;

        this.Trace("init monitor");
        _connectionMonitor = monitor;
        _connectionMonitor.Init(this);

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    public ClientWebSocket()
        : this(ConnectionMonitor.None)
    {
    }

    public void Connect(Uri uri)
    {
        this.Trace("start");

        if (_status is Status.Connecting or Status.Connected)
        {
            this.Trace($"skip - already {_status}");
            return;
        }

        SetStatus(Status.Connecting);
        ConnectPrivate(uri);

        this.Trace("done");
    }

    public void Disconnect()
    {
        this.Trace("start");

        if (_status is Status.Disconnected)
        {
            this.Trace($"skip - already {_status}");
            return;
        }

        SetStatus(Status.Disconnected);

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace("disconnect managed socket");
        _socket.DisconnectAsync();

        this.Trace("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");
        return _socket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");
        return _socket.SendBinaryAsync(data, ct);
    }

    private void ReconnectPrivate(Uri uri, WebSocketCloseStatus closeStatus)
    {
        this.Trace("start");

        SetStatus(Status.Connecting);

        this.Trace("fire disconnected");
        OnDisconnected(closeStatus);

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace("trigger connect");
        ConnectPrivate(uri);

        this.Trace("done");
    }

    private void ConnectPrivate(Uri uri)
    {
        this.Trace("start");

        _uri = uri;
        _socket.ConnectAsync(uri, CancellationToken.None).ContinueWith(HandleOpened);

        this.Trace("done");
    }

    private void HandleOpened(Task task)
    {
        this.Trace("start");

        if (_status is Status.Connected or Status.Disconnected)
        {
            this.Trace($"skip - already {_status}");
            return;
        }

        SetStatus(Status.Connected);

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("fire connected");
        OnConnected();
    }

    private void HandleConnectionLost()
    {
        this.Trace("start");

        if (_status is Status.Disconnected)
        {
            this.Trace($"skip - already {_status}");
            return;
        }

        ReconnectPrivate(_uri!, WebSocketCloseStatus.ClosedRemote);
    }

    private void HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start");

        if (_status is Status.Disconnected)
        {
            this.Trace($"skip - already {_status}");
            return;
        }

        ReconnectPrivate(_uri!, task.Result.Status);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace($"update status from {_status} to {status}");
        _status = status;
    }

    private enum Status
    {
        Disconnected,
        Connecting,
        Connected,
    }
}