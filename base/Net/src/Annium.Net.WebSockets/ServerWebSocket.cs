using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public class ServerWebSocket : IServerWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };

    public Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<WebSocketCloseStatus> ListenAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}