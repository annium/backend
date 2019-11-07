using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
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

        public Task<IBooleanResult> SendAsync<T>(T data, CancellationToken token) =>
            SendAsync(data.Serialize(Format), token);

        public Task<IBooleanResult> SendAsync(string data, CancellationToken token) =>
            SendAsync(encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public Task<IBooleanResult> SendAsync(ReadOnlyMemory<byte> data, CancellationToken token) =>
             SendAsync(data, WebSocketMessageType.Binary, token);


        public Task<IBooleanResult<SocketResponse<T>>> ReceiveAsync<T>(CancellationToken token) =>
            ReceiveAsync(WebSocketMessageType.Binary, x => x.Deserialize<T>(Format), token);

        public Task<IBooleanResult<SocketResponse<string>>> ReceiveTextAsync(CancellationToken token) =>
            ReceiveAsync(WebSocketMessageType.Binary, encoding.GetString, token);

        public Task<IBooleanResult<SocketResponse<byte[]>>> ReceiveBinaryAsync(CancellationToken token) =>
            ReceiveAsync(WebSocketMessageType.Binary, x => x, token);

        private async Task<IBooleanResult> SendAsync(ReadOnlyMemory<byte> data, WebSocketMessageType messageType, CancellationToken token)
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

                return Result.Success();
            }
            // is thrown, if remote party closed connection w/o handshake
            catch (Exception e)
            {
                return Result.Failure().Error(e.Message);
            }
        }

        private async Task<IBooleanResult<SocketResponse<T>>> ReceiveAsync<T>(
            WebSocketMessageType? type,
            Func<byte[], T> convert,
            CancellationToken token
        )
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

                        return Result.Success(new SocketResponse<T>(false, convert(Array.Empty<byte>())));
                    }

                    // skip unexpected message type
                    if (result.MessageType == type)
                        continue;


                    stream.Write(mem.Slice(0, result.Count).Span);
                }
                while (!result.EndOfMessage);

                return Result.Success(new SocketResponse<T>(true, convert(stream.ToArray())));
            }
            // is thrown, if remote party closed connection w/o handshake
            catch (WebSocketException)
            {
                return Result.Success(new SocketResponse<T>(false, convert(Array.Empty<byte>())));
            }
            catch (Exception e)
            {
                return Result.Failure(new SocketResponse<T>(false, convert(Array.Empty<byte>()))).Error(e.Message);
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