using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

internal class Channel
{
    private static Task DelayAsync() => Task.Delay(10);

    private static void Delay(Action handle) => DelayAsync().ContinueWith(_ => handle());

    public event Action OnConnected = delegate { };
    public event Action<CloseSide> OnDisconnected = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnClientReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnServerReceived = delegate { };
    private readonly object _locker = new();
    private bool _isConnected;

    public void Connect()
    {
        lock (_locker)
        {
            if (_isConnected)
                throw new InvalidOperationException("Channel is already connected");
            _isConnected = true;
        }

        Delay(() => OnConnected());
    }

    public void Disconnect(CloseSide side)
    {
        lock (_locker)
        {
            if (!_isConnected)
                return;
        }

        OnDisconnected(side);
    }

    public ValueTask<ConnectionSendStatus> SendToServerAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return ValueTask.FromResult(ConnectionSendStatus.Canceled);

        lock (_locker)
            if (!_isConnected)
                return ValueTask.FromResult(ConnectionSendStatus.Closed);

        OnServerReceived(data);

        return ValueTask.FromResult(ConnectionSendStatus.Ok);
    }

    public ValueTask<ConnectionSendStatus> SendToClientAsync(ReadOnlyMemory<byte> data, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return ValueTask.FromResult(ConnectionSendStatus.Canceled);

        lock (_locker)
            if (!_isConnected)
                return ValueTask.FromResult(ConnectionSendStatus.Closed);

        OnClientReceived(data);

        return ValueTask.FromResult(ConnectionSendStatus.Ok);
    }
}

internal enum CloseSide
{
    Client,
    Server,
}
