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
    public abstract class WebSocketBase<TNativeSocket> : ISendingReceivingWebSocket
        where TNativeSocket : NativeWebSocket
    {
        private const int BufferSize = 65536;

        public bool IsConnected => State == WebSocketState.Open || State == WebSocketState.CloseReceived || State == WebSocketState.CloseSent;
        public WebSocketState State => Socket.State;

        protected TNativeSocket Socket { get; set; }
        private readonly ISerializer<byte[]> serializer;
        private readonly UTF8Encoding encoding = new UTF8Encoding();
        private readonly IObservable<SocketData> socketObservable;

        internal WebSocketBase(
            TNativeSocket socket,
            ISerializer<byte[]> serializer
        )
        {
            Socket = socket;
            this.serializer = serializer;
            socketObservable = CreateSocketObservable();
        }

        public IObservable<int> Send<T>(T data, CancellationToken token) =>
            Send(encoding.GetString(serializer.Serialize(data)), token);

        public IObservable<int> Send(string data, CancellationToken token) =>
            Send(encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public IObservable<int> Send(ReadOnlyMemory<byte> data, CancellationToken token) =>
            Send(data, WebSocketMessageType.Binary, token);

        public IObservable<T> Listen<T>() where T : notnull => socketObservable
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => serializer.Deserialize<T>(x.Data.ToArray()));

        public IObservable<string> ListenText() => socketObservable
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => encoding.GetString(x.Data.Span));

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() => socketObservable
            .Where(x => x.Type == WebSocketMessageType.Binary)
            .Select(x => x.Data);

        protected abstract Task OnDisconnectAsync();

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

                    // if closing - handle disconnect
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        OnDisconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                        // if after disconnect handling not connected - set completed and break
                        if (!IsConnected)
                        {
                            observer.OnCompleted();

                            return false;
                        }
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

        public void Dispose()
        {
            Socket.Dispose();
        }

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