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
    private readonly IServerManagedWebSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Status _status = Status.Connected;

    public ServerWebSocket(NativeWebSocket nativeSocket, IConnectionMonitor monitor, CancellationToken ct = default)
    {
        this.Trace("start");
        _socket = new ServerManagedWebSocket(nativeSocket, ct);
        _socket.TextReceived += TextReceived;
        _socket.BinaryReceived += BinaryReceived;

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("init monitor");
        _connectionMonitor = monitor;
        _connectionMonitor.Init(this);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += Disconnect;
    }

    public ServerWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
        : this(nativeSocket, ConnectionMonitor.None, ct)
    {
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

    private void HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start");

        if (_status is Status.Disconnected)
        {
            this.Trace($"skip - already {_status}");
            return;
        }

        SetStatus(Status.Disconnected);

        var result = task.Result;
        if (result.Exception is not null)
        {
            this.Trace($"fire error: {result.Exception}");
            OnError(result.Exception);
        }

        this.Trace("fire disconnected");
        OnDisconnected(result.Status);

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
        Connected,
    }
}