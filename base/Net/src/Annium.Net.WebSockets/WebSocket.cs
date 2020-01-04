using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public abstract class WebSocket<TNativeSocket> : ISendingWebSocket, IReceivingWebSocket, IDisposable
        where TNativeSocket : NativeWebSocket
    {
        private const int BufferSize = 65536;

        protected readonly TNativeSocket Socket;
        private readonly ISerializer<ReadOnlyMemory<byte>> serializer;
        private readonly UTF8Encoding encoding = new UTF8Encoding();
        private readonly IObservable<SocketData> socketObservable;

        public WebSocket(
            TNativeSocket socket,
            ISerializer<ReadOnlyMemory<byte>> serializer
        )
        {
            Socket = socket;
            this.serializer = serializer;
            socketObservable = CreateSocketObservable();
        }

        public IObservable<int> Send<T>(T data, CancellationToken token) =>
            Send(serializer.Serialize(data), token);

        public IObservable<int> Send(string data, CancellationToken token) =>
            Send(encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public IObservable<int> Send(ReadOnlyMemory<byte> data, CancellationToken token) =>
            Send(data, WebSocketMessageType.Binary, token);

        public IObservable<T> Listen<T>() where T : notnull => socketObservable
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => serializer.Deserialize<T>(x.Data));

        public IObservable<string> ListenText() => socketObservable
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => encoding.GetString(x.Data.Span));

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() => socketObservable
            .Where(x => x.Type == WebSocketMessageType.Binary)
            .Select(x => x.Data);

        private IObservable<int> Send(
            ReadOnlyMemory<byte> data,
            WebSocketMessageType messageType,
            CancellationToken token
        ) => Observable.FromAsync(async () =>
        {
            // TODO: implement chunking, if needed
            await Socket.SendAsync(
                buffer: data,
                messageType: messageType,
                endOfMessage: true,
                cancellationToken: token
            );

            return data.Length;
        });

        private IObservable<SocketData> CreateSocketObservable() =>
            Observable.Create<SocketData>(async (observer, token) =>
            {
                var pool = ArrayPool<byte>.Shared;
                var buffer = pool.Rent(BufferSize);

                while (await ReceiveAsync(observer, buffer, token))
                {
                }

                pool.Return(buffer);

                return () => Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            }).Publish().RefCount();

        private async ValueTask<bool> ReceiveAsync(
            IObserver<SocketData> observer,
            Memory<byte> buffer,
            CancellationToken token
        )
        {
            try
            {
                using var stream = new MemoryStream();
                ValueWebSocketReceiveResult result;
                do
                {
                    result = await Socket.ReceiveAsync(buffer, token);

                    // if closing - send close and return
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _ = Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);

                        observer.OnCompleted();

                        return false;
                    }

                    stream.Write(buffer.Slice(0, result.Count).Span);
                } while (!result.EndOfMessage);

                observer.OnNext(new SocketData(result.MessageType, stream.ToArray()));

                return true;
            }
            //  remote party closed connection w/o handshake
            catch (WebSocketException)
            {
                observer.OnCompleted();

                return false;
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                observer.OnCompleted();

                return false;
            }
            catch (Exception e)
            {
                observer.OnError(e);

                return false;
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
                Socket.Dispose();

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        private struct SocketData
        {
            public WebSocketMessageType Type { get; }
            public ReadOnlyMemory<byte> Data { get; }

            public SocketData(
                WebSocketMessageType type,
                ReadOnlyMemory<byte> data
            )
            {
                Type = type;
                Data = data;
            }
        }
    }
}