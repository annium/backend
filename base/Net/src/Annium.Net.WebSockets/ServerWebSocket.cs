using System;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

public class ServerWebSocket : IServerWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    private readonly IServerManagedWebSocket _managedSocket;

    public ServerWebSocket(NativeWebSocket nativeSocket, CancellationToken ct = default)
    {
        _managedSocket = new ServerManagedWebSocket(nativeSocket, ct);
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public event Action<WebSocketCloseStatus>? OnDisconnected;
    public event Action<Exception>? OnError;

    public void Disconnect()
    {
        throw new NotImplementedException();
    }
}