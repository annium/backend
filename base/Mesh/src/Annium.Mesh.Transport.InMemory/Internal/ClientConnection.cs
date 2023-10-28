using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

internal class ClientConnection : IClientConnection
{
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private readonly Channel _channel;

    public ClientConnection(Channel channel)
    {
        _channel = channel;
        _channel.OnConnected += () => OnConnected();
        _channel.OnDisconnected += side =>
            OnDisconnected(
                side is CloseSide.Client ? ConnectionCloseStatus.ClosedLocal : ConnectionCloseStatus.ClosedRemote
            );
        _channel.OnClientReceived += data => OnReceived(data);
    }

    public void Connect()
    {
        _channel.Connect();
    }

    public void Disconnect()
    {
        _channel.Disconnect(CloseSide.Client);
    }

    public ValueTask<ConnectionSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return _channel.SendToServerAsync(data, ct);
    }
}
