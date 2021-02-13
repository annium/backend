using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly UTF8Encoding _encoding = new UTF8Encoding();
        private readonly IObservable<SocketMessage> _socketObservable;

        internal WebSocketBase(
            TNativeSocket socket
        )
        {
            Socket = socket;
            _socketObservable = CreateSocketObservable();
        }

        public IObservable<int> Send(string data, CancellationToken token) =>
            Send(_encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public IObservable<int> Send(ReadOnlyMemory<byte> data, CancellationToken token) =>
            Send(data, WebSocketMessageType.Binary, token);

        public IObservable<SocketMessage> Listen() => _socketObservable;

        public IObservable<string> ListenText() => Listen()
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => _encoding.GetString(x.Data.Span));

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() => _socketObservable
            .Where(x => x.Type == WebSocketMessageType.Binary)
            .Select(x => x.Data);

        protected abstract Task OnDisconnectAsync();

        private IObservable<int> Send(
            ReadOnlyMemory<byte> data,
            WebSocketMessageType messageType,
            CancellationToken ct
        ) => Observable.FromAsync(async () =>
        {
            // TODO: implement chunking, if needed
            await Socket.SendAsync(
                buffer: data,
                messageType: messageType,
                endOfMessage: true,
                cancellationToken: ct
            );

            return data.Length;
        });

        private IObservable<SocketMessage> CreateSocketObservable() =>
            Observable.Create<SocketMessage>(async (observer, token) =>
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
            IObserver<SocketMessage> observer,
            Memory<byte> buffer,
            CancellationToken token
        )
        {
            try
            {
                await using var stream = new MemoryStream();
                ValueWebSocketReceiveResult result;
                do
                {
                    result = await Socket.ReceiveAsync(buffer, token);

                    // if closing - handle disconnect
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await OnDisconnectAsync().ConfigureAwait(false);

                        // if after disconnect handling not connected - set completed and break
                        if (!IsConnected)
                        {
                            observer.OnCompleted();

                            return false;
                        }
                    }

                    stream.Write(buffer.Slice(0, result.Count).Span);
                } while (!result.EndOfMessage);

                observer.OnNext(new SocketMessage(result.MessageType, stream.ToArray()));

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
    }
}