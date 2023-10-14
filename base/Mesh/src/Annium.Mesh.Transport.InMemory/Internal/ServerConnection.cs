using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

internal class ServerConnection : IServerConnection
{
    public Guid Id { get; } = Guid.NewGuid();
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private readonly Channel _channel;

    public ServerConnection(Channel channel)
    {
        _channel = channel;
        _channel.OnDisconnected += side => OnDisconnected(side is CloseSide.Client ? ConnectionCloseStatus.ClosedRemote : ConnectionCloseStatus.ClosedLocal);
        _channel.OnServerReceived += data => OnReceived(data);
    }

    public void Disconnect()
    {
        _channel.Disconnect(CloseSide.Server);
    }

    public ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return _channel.SendToClientAsync(data, ct);
    }
}