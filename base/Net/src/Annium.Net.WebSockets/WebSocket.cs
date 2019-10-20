using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public abstract class WebSocket<TNativeSocket> : ISendingWebSocket, IReceivingWebSocket, IDisposable
    where TNativeSocket : NativeWebSocket
    {
        private const int bufferSize = 65536;

        public MessageFormat Format { get; }
        protected readonly TNativeSocket socket;
        private readonly UTF8Encoding encoding = new UTF8Encoding();

        public WebSocket(
            TNativeSocket socket,
            MessageFormat format
        )
        {
            this.socket = socket;
            Format = format;
        }

        public Task SendAsync<T>(T data, CancellationToken token)
        {
            return SendAsync(data.Serialize(Format), token);
        }

        public Task SendAsync(string data, CancellationToken token)
        {
            return SendAsync(encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);
        }

        public Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token)
        {
            return SendAsync(data, WebSocketMessageType.Binary, token);
        }

        public async Task<(bool isClosed, T data)> ReceiveAsync<T>(CancellationToken token)
        {
            while (true)
            {
                var (type, data) = await ReceiveAsync(token);

                if (type == WebSocketMessageType.Close)
                    return (true, default);

                if (type == WebSocketMessageType.Text)
                    return (false, data.Deserialize<T>(Format));
            }
        }

        public async Task<(bool isClosed, string data)> ReceiveTextAsync(CancellationToken token)
        {
            while (true)
            {
                var (type, data) = await ReceiveAsync(token);

                if (type == WebSocketMessageType.Close)
                    return (true, string.Empty);

                if (type == WebSocketMessageType.Text)
                    return (false, encoding.GetString(data));
            }
        }

        public async Task<(bool isClosed, byte[] data)> ReceiveBinaryAsync(CancellationToken token)
        {
            while (true)
            {
                var (type, data) = await ReceiveAsync(token);

                if (type == WebSocketMessageType.Close)
                    return (true, Array.Empty<byte>());

                if (type == WebSocketMessageType.Binary)
                    return (false, data);
            }
        }

        private async Task SendAsync(ReadOnlyMemory<byte> data, WebSocketMessageType messageType, CancellationToken token)
        {
            try
            {
                // TODO: implement chunking, if needed
                await socket.SendAsync(
                    buffer: data,
                    messageType: messageType,
                    endOfMessage: true,
                    cancellationToken: token
                );
            }
            // is thrown, if remote party closed connection w/o handshake
            catch (WebSocketException)
            {

            }
        }

        private async Task<(WebSocketMessageType type, byte[] data)> ReceiveAsync(CancellationToken token)
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(bufferSize);
            var mem = new Memory<byte>(buffer);
            using var stream = new MemoryStream();

            try
            {
                ValueWebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(mem, token);

                    // if closing - send close and return
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _ = socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);

                        return (result.MessageType, Array.Empty<byte>());
                    }

                    stream.Write(mem.Slice(0, result.Count).Span);
                }
                while (!result.EndOfMessage);

                return (result.MessageType, stream.ToArray());
            }
            // is thrown, if remote party closed connection w/o handshake
            catch (WebSocketException)
            {
                return (WebSocketMessageType.Close, Array.Empty<byte>());
            }
            finally
            {
                pool.Return(buffer);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
                socket.Dispose();

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}