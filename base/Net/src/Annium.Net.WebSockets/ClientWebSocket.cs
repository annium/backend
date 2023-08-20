using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets;

public class ClientWebSocket : IClientWebSocket
{
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    private NativeWebSocket? _nativeSocket;
    private ManagedWebSocket? _managedSocket;

    public async ValueTask ConnectAsync(Uri uri, CancellationToken ct)
    {
        EnsureNotConnected();

        var nativeSocket = new NativeWebSocket();
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket);
        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

        await nativeSocket.ConnectAsync(uri, ct).ConfigureAwait(false);
    }

    public async ValueTask DisconnectAsync()
    {
        EnsureConnected();

        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

        await _nativeSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        return IsConnected() ? _managedSocket.SendTextAsync(text, ct) : ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return IsConnected() ? _managedSocket.SendBinaryAsync(data, ct) : ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    public ValueTask<WebSocketReceiveStatus> ListenAsync(CancellationToken ct)
    {
        return IsConnected() ? _managedSocket.ListenAsync(ct) : ValueTask.FromResult(WebSocketReceiveStatus.ClosedLocal);
    }

    private void OnTextReceived(ReadOnlyMemory<byte> data) => TextReceived(data);
    private void OnBinaryReceived(ReadOnlyMemory<byte> data) => BinaryReceived(data);

    private void EnsureNotConnected()
    {
        if (_nativeSocket is not null || _managedSocket is not null)
            throw new InvalidOperationException("Socket is already connected");
    }

    [MemberNotNull(nameof(_nativeSocket), nameof(_managedSocket))]
    private void EnsureConnected()
    {
        if (_nativeSocket is null || _managedSocket is null)
            throw new InvalidOperationException("Socket is not connected");
    }

    [MemberNotNullWhen(true, nameof(_nativeSocket), nameof(_managedSocket))]
    private bool IsConnected()
    {
        return _nativeSocket is not null && _managedSocket is not null;
    }
}