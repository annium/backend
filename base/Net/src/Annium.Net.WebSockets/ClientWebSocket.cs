using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

public class ClientWebSocket : IClientWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public event Action OnConnected = delegate { };
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly object _locker = new();
    private readonly IClientManagedWebSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Uri? _uri;
    private Status _status = Status.Disconnected;
    private readonly int _reconnectDelay;

    public ClientWebSocket(ClientWebSocketOptions options)
    {
        this.Trace("start monitor");
        _socket = new ClientManagedWebSocket();
        this.Trace($"paired with {_socket.GetFullId()}");

        this.Trace("bind events");
        _socket.TextReceived += TextReceived;
        _socket.BinaryReceived += BinaryReceived;

        this.Trace("init monitor");
        _connectionMonitor = options.ConnectionMonitor;
        _connectionMonitor.Init(this);
        _reconnectDelay = options.ReconnectDelay;

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    public ClientWebSocket()
        : this(ClientWebSocketOptions.Default)
    {
    }

    public void Connect(Uri uri)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Connecting or Status.Connected)
            {
                this.Trace($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Connecting);
        }

        ConnectPrivate(uri);

        this.Trace("done");
    }

    public void Disconnect()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Disconnected);
        }

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace("disconnect managed socket");
        _socket.DisconnectAsync();

        this.Trace("fire disconnected");
        OnDisconnected(WebSocketCloseStatus.ClosedLocal);

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

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace($"fire disconnected with {closeStatus}");
        OnDisconnected(closeStatus);

        this.Trace($"schedule connection in {_reconnectDelay}ms");
        Task.Delay(_reconnectDelay).ContinueWith(_ =>
        {
            this.Trace("trigger connect");
            ConnectPrivate(uri);

            this.Trace("done");
        });
    }

    private void ConnectPrivate(Uri uri)
    {
        this.Trace("start");

        _uri = uri;
        this.Trace($"connect to {uri}");
        _socket.ConnectAsync(uri, CancellationToken.None).ContinueWith(HandleConnected, uri);

        this.Trace("done");
    }

    private void HandleConnected(Task<bool> task, object? state)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Connected or Status.Disconnected)
            {
                this.Trace($"skip - already {_status}");
                return;
            }

            // set status in lock
            this.Trace($"set status by connection result: {task.Result}");
            SetStatus(task.Result ? Status.Connected : Status.Connecting);
        }

        if (!task.Result)
        {
            var uri = (Uri)state!;
            this.Trace($"failure: {task.Exception}, init reconnect");
            ReconnectPrivate(uri, WebSocketCloseStatus.Error);
            return;
        }

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("fire connected");
        OnConnected();

        this.Trace("done");
    }

    private void HandleConnectionLost()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Connecting);
        }

        ReconnectPrivate(_uri!, WebSocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    private void HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Connecting);
        }

        ReconnectPrivate(_uri!, task.Result.Status);

        this.Trace("done");
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