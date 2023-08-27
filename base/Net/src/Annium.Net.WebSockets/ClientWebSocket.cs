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
    private readonly object _locker = new();
    private readonly IClientManagedWebSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Uri? _uri;
    private Status _status = Status.Disconnected;
    private readonly int _reconnectDelay;

    public ClientWebSocket(ClientWebSocketOptions options)
    {
        this.TraceOld("start monitor");
        _socket = new ClientManagedWebSocket();
        this.TraceOld($"paired with {_socket.GetFullId()}");

        this.TraceOld("bind events");
        _socket.TextReceived += TextReceived;
        _socket.BinaryReceived += BinaryReceived;

        this.TraceOld("init monitor");
        _connectionMonitor = options.ConnectionMonitor;
        _connectionMonitor.Init(this);
        _reconnectDelay = options.ReconnectDelay;

        this.TraceOld("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    public ClientWebSocket()
        : this(ClientWebSocketOptions.Default)
    {
    }

    public void Connect(Uri uri)
    {
        this.TraceOld("start");

        lock (_locker)
        {
            if (_status is Status.Connecting or Status.Connected)
            {
                this.TraceOld($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Connecting);
        }

        ConnectPrivate(uri);

        this.TraceOld("done");
    }

    public void Disconnect()
    {
        this.TraceOld("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.TraceOld($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Disconnected);
        }

        this.TraceOld("stop monitor");
        _connectionMonitor.Stop();

        this.TraceOld("disconnect managed socket");
        _socket.DisconnectAsync();

        this.TraceOld("fire disconnected");
        OnDisconnected(WebSocketCloseStatus.ClosedLocal);

        this.TraceOld("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.TraceOld("send text");
        return _socket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.TraceOld("send binary");
        return _socket.SendBinaryAsync(data, ct);
    }

    private void ReconnectPrivate(Uri uri, WebSocketCloseStatus closeStatus)
    {
        this.TraceOld("start");

        this.TraceOld("stop monitor");
        _connectionMonitor.Stop();

        this.TraceOld($"fire disconnected with {closeStatus}");
        OnDisconnected(closeStatus);

        this.TraceOld($"schedule connection in {_reconnectDelay}ms");
        Task.Delay(_reconnectDelay).ContinueWith(_ =>
        {
            this.TraceOld("trigger connect");
            ConnectPrivate(uri);

            this.TraceOld("done");
        });
    }

    private void ConnectPrivate(Uri uri)
    {
        this.TraceOld("start");

        _uri = uri;
        this.TraceOld($"connect to {uri}");
        _socket.ConnectAsync(uri, CancellationToken.None).ContinueWith(HandleConnected, uri);

        this.TraceOld("done");
    }

    private void HandleConnected(Task<bool> task, object? state)
    {
        this.TraceOld("start");

        lock (_locker)
        {
            if (_status is Status.Connected or Status.Disconnected)
            {
                this.TraceOld($"skip - already {_status}");
                return;
            }

            // set status in lock
            this.TraceOld($"set status by connection result: {task.Result}");
            SetStatus(task.Result ? Status.Connected : Status.Connecting);
        }

        if (!task.Result)
        {
            var uri = (Uri)state!;
            this.TraceOld($"failure: {task.Exception}, init reconnect");
            ReconnectPrivate(uri, WebSocketCloseStatus.Error);
            return;
        }

        this.TraceOld("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.TraceOld("start monitor");
        _connectionMonitor.Start();

        this.TraceOld("fire connected");
        OnConnected();

        this.TraceOld("done");
    }

    private void HandleConnectionLost()
    {
        this.TraceOld("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.TraceOld($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Connecting);
        }

        ReconnectPrivate(_uri!, WebSocketCloseStatus.ClosedRemote);

        this.TraceOld("done");
    }

    private void HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.TraceOld("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.TraceOld($"skip - already {_status}");
                return;
            }

            SetStatus(Status.Connecting);
        }

        ReconnectPrivate(_uri!, task.Result.Status);

        this.TraceOld("done");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.TraceOld($"update status from {_status} to {status}");
        _status = status;
    }

    private enum Status
    {
        Disconnected,
        Connecting,
        Connected,
    }
}