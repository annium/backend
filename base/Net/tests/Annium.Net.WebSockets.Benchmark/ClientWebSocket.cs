using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Benchmark;

public class ClientWebSocket : IClientWebSocket
{
    public event TextMessageHandler? TextReceived;
    public event BinaryMessageHandler? BinaryReceived;

    public Task ConnectAsync(Uri uri, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

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