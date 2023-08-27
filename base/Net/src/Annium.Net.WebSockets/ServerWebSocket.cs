using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

public class ServerWebSocket : IServerWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly object _locker = new();
    private readonly IServerManagedWebSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Status _status = Status.Connected;

    public ServerWebSocket(NativeWebSocket nativeSocket, ServerWebSocketOptions options, CancellationToken ct = default)
    {
        this.TraceOld("start");
        _socket = new ServerManagedWebSocket(nativeSocket, ct);
        _socket.TextReceived += TextReceived;
        _socket.BinaryReceived += BinaryReceived;

        this.TraceOld("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.TraceOld("init monitor");
        _connectionMonitor = options.ConnectionMonitor;
        _connectionMonitor.Init(this);

        this.TraceOld("start monitor");
        _connectionMonitor.Start();

        this.TraceOld("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += Disconnect;
    }

    public ServerWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
        : this(nativeSocket, ServerWebSocketOptions.Default, ct)
    {
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

            SetStatus(Status.Disconnected);
        }

        this.TraceOld("stop monitor");
        _connectionMonitor.Stop();

        var result = task.Result;
        if (result.Exception is not null)
        {
            this.TraceOld($"fire error: {result.Exception}");
            OnError(result.Exception);
        }

        this.TraceOld("fire disconnected");
        OnDisconnected(result.Status);

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
        Connected,
    }
}