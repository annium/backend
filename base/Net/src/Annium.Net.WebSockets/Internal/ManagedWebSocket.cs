using System;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Debug;

namespace Annium.Net.WebSockets.Internal;

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

    public async Task<WebSocketCloseResult> ListenAsync(CancellationToken ct)
    {
        using var buffer = new DynamicBuffer<byte>(BufferSize);

        while (true)
        {
            var (isClosed,result) = await ReceiveAsync(buffer, ct);
            if (isClosed)
            {
                this.Trace(result.Exception is not null ? $"stop with {result.Status}: {result.Exception}" : $"stop with {result.Status}");
                return result;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<WebSocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, WebSocketMessageType messageType, CancellationToken ct = default)
    {
        try
        {
            this.Trace($"{messageType} ({data.Length}) - start");

            if (ct.IsCancellationRequested)
            {
                this.Trace($"{messageType} ({data.Length}) - canceled with cancellation token");
                return WebSocketSendStatus.Canceled;
            }

            if (_socket.State is not WebSocketState.Open)
            {
                this.Trace($"{messageType} ({data.Length}) - closed because socket is not open");
                return WebSocketSendStatus.Closed;
            }

            await _socket.SendAsync(data, messageType, true, ct).ConfigureAwait(false);
            this.Trace($"{messageType} ({data.Length}) - send succeed");

            return WebSocketSendStatus.Ok;
        }
        catch (OperationCanceledException)
        {
            this.Trace($"{messageType} ({data.Length}) - canceled with OperationCanceledException");
            return WebSocketSendStatus.Canceled;
        }
        catch (InvalidOperationException e)
        {
            this.Trace($"{messageType} ({data.Length}) - closed with InvalidOperationException: {e}");
            return WebSocketSendStatus.Closed;
        }
        catch (WebSocketException e)
        {
            this.Trace($"{messageType} ({data.Length}) - closed with WebSocketException: {e}");
            return WebSocketSendStatus.Closed;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<(bool IsClosed, WebSocketCloseResult Result)> ReceiveAsync(DynamicBuffer<byte> buffer, CancellationToken ct)
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
                return (true, new WebSocketCloseResult(receiveResult.Status, receiveResult.Exception));
            }

            // track receiveResult count
            buffer.TrackDataSize(receiveResult.Count);

            // buffer was not big enough - grow and receive next chunk
            if (!receiveResult.EndOfMessage)
            {
                this.Trace("grow buffer");
                buffer.Grow();
                continue;
            }

            this.Trace($"fire {receiveResult.MessageType} received");
            if (receiveResult.MessageType is WebSocketMessageType.Text)
                TextReceived(buffer.AsDataReadOnlyMemory());
            else
                BinaryReceived(buffer.AsDataReadOnlyMemory());

            return (false, new WebSocketCloseResult(WebSocketCloseStatus.ClosedRemote, null));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<ReceiveResult> ReceiveChunkAsync(DynamicBuffer<byte> buffer, CancellationToken ct)
    {
        try
        {
            if (ct.IsCancellationRequested)
            {
                this.Trace("canceled with cancellation token");
                return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedLocal, null);
            }

            if (_socket.State is not WebSocketState.Open)
            {
                this.Trace("closed because socket is not open");
                return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedLocal, null);
            }

            var result = await _socket.ReceiveAsync(buffer.AsFreeSpaceMemory(), ct).ConfigureAwait(false);
            this.Trace($"received {result.MessageType} ({result.Count} - {result.EndOfMessage})");

            return new ReceiveResult(result.MessageType, result.Count, result.EndOfMessage, WebSocketCloseStatus.ClosedRemote, null);
        }
        catch (OperationCanceledException)
        {
            this.Trace($"closed locally with cancellation: {ct.IsCancellationRequested}");
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedLocal, null);
        }
        catch (WebSocketException e)
        {
            this.Trace($"closed remotely with WebSocketException: {e}");
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedRemote, null);
        }
        catch (Exception e)
        {
            this.Trace($"Error!!: {e}");
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.Error, e);
        }
    }
}