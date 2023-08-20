using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

public class ManagedWebSocket : ISendingReceivingWebSocket
{
    private const int BufferSize = 65_536;
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    private readonly WebSocket _socket;

    public ManagedWebSocket(WebSocket socket)
    {
        _socket = socket;
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        return SendAsync(text, WebSocketMessageType.Text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return SendAsync(data, WebSocketMessageType.Binary, ct);
    }

    public async Task<WebSocketReceiveStatus> ListenAsync(CancellationToken ct)
    {
        using var buffer = new DynamicBuffer<byte>(BufferSize);

        while (true)
        {
            var result = await ReceiveAsync(buffer, ct);
            if (result.IsClosed)
            {
                return result.CloseStatus;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<WebSocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, WebSocketMessageType messageType, CancellationToken ct = default)
    {
        try
        {
            if (ct.IsCancellationRequested)
                return WebSocketSendStatus.Canceled;

            if (_socket.State is not WebSocketState.Open)
                return WebSocketSendStatus.Closed;

            await _socket.SendAsync(data, messageType, true, ct).ConfigureAwait(false);

            return WebSocketSendStatus.Ok;
        }
        catch (OperationCanceledException)
        {
            return WebSocketSendStatus.Canceled;
        }
        catch (InvalidOperationException)
        {
            return WebSocketSendStatus.Closed;
        }
        catch (WebSocketException)
        {
            return WebSocketSendStatus.Closed;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<(bool IsClosed, WebSocketReceiveStatus CloseStatus)> ReceiveAsync(DynamicBuffer<byte> buffer, CancellationToken ct)
    {
        // reset buffer to start writing from start
        buffer.Reset();

        while (true)
        {
            // read chunk into buffer
            var receiveResult = await ReceiveChunkAsync(buffer, ct).ConfigureAwait(false);

            // if close received - return false, indicating socket is closed
            if (receiveResult.MessageType is WebSocketMessageType.Close)
            {
                return (true, receiveResult.Status);
            }

            // track receiveResult count
            buffer.TrackDataSize(receiveResult.Count);

            // buffer was not big enough - grow and receive next chunk
            if (!receiveResult.EndOfMessage)
            {
                buffer.Grow();
                continue;
            }

            if (receiveResult.MessageType is WebSocketMessageType.Text)
                TextReceived(buffer.AsDataReadOnlyMemory());
            else
                BinaryReceived(buffer.AsDataReadOnlyMemory());

            return (false, WebSocketReceiveStatus.Normal);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<ReceiveResult> ReceiveChunkAsync(DynamicBuffer<byte> buffer, CancellationToken ct)
    {
        try
        {
            if (ct.IsCancellationRequested)
                return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketReceiveStatus.Canceled);

            if (_socket.State is not WebSocketState.Open)
                return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketReceiveStatus.ClosedLocal);

            var result = await _socket.ReceiveAsync(buffer.AsFreeSpaceMemory(), ct).ConfigureAwait(false);

            return new ReceiveResult(result.MessageType, result.Count, result.EndOfMessage, WebSocketReceiveStatus.Normal);
        }
        catch (OperationCanceledException)
        {
            var status = ct.IsCancellationRequested ? WebSocketReceiveStatus.Canceled : WebSocketReceiveStatus.ClosedLocal;
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, status);
        }
        catch (WebSocketException)
        {
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketReceiveStatus.ClosedRemote);
        }
        catch (Exception)
        {
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketReceiveStatus.Error);
        }
    }
}