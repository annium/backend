using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive;
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

        public IObservable<Unit> Send(string data, CancellationToken token) =>
            Send(_encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public IObservable<Unit> Send(ReadOnlyMemory<byte> data, CancellationToken token) =>
            Send(data, WebSocketMessageType.Binary, token);

        public IObservable<SocketMessage> Listen() => _socketObservable;

        public IObservable<string> ListenText() => _socketObservable
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => _encoding.GetString(x.Data.Span));

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() => _socketObservable
            .Where(x => x.Type == WebSocketMessageType.Binary)
            .Select(x => x.Data);

        protected abstract Task OnDisconnectAsync();

        private IObservable<Unit> Send(
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

            return Unit.Default;
        });

        private IObservable<SocketMessage> CreateSocketObservable() =>
            ObservableInstance.Create<SocketMessage>(async ctx =>
            {
                var pool = ArrayPool<byte>.Shared;
                var buffer = pool.Rent(BufferSize);

                // Debug($"INTERNAL: Resume in {Socket.State} state");

                while (
                    !ctx.Token.IsCancellationRequested &&
                    (
                        Socket.State == WebSocketState.Open ||
                        Socket.State == WebSocketState.CloseSent
                    )
                )
                    await ReceiveAsync(ctx, buffer);

                pool.Return(buffer);

                // Debug($"INTERNAL: Suspend in {Socket.State} state");
            });

        private async ValueTask ReceiveAsync(
            ObserverContext<SocketMessage> ctx,
            Memory<byte> buffer
        )
        {
            try
            {
                await using var stream = new MemoryStream();
                ValueWebSocketReceiveResult result;
                do
                {
                    // Debug($"INTERNAL: Receive in State {Socket.State}");
                    result = await Socket.ReceiveAsync(buffer, CancellationToken.None);
                    // Debug($"INTERNAL: Received {result.MessageType} in State {Socket.State}");

                    // if closing - handle disconnect
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await OnDisconnectAsync().ConfigureAwait(false);

                        // if after disconnect handling not connected - set completed and break
                        if (
                            State == WebSocketState.CloseReceived ||
                            State == WebSocketState.Closed ||
                            State == WebSocketState.Aborted
                        )
                        {
                            ctx.OnCompleted();
                        }
                    }

                    stream.Write(buffer.Slice(0, result.Count).Span);
                } while (!result.EndOfMessage);

                ctx.OnNext(new SocketMessage(result.MessageType, stream.ToArray()));
            }
            //  remote party closed connection w/o handshake
            catch (WebSocketException e)
            {
                // Debug($"INTERNAL: WebSocketException {e}");
                ctx.OnCompleted();
            }
            // token was canceled, or connection was aborted during Receive operation
            catch (OperationCanceledException e)
            {
                // Debug($"INTERNAL: OperationCanceledException {e}");
                ctx.OnCompleted();
            }
            catch (Exception e)
            {
                // Debug($"INTERNAL: Exception {e}");
                ctx.OnError(e);
            }
        }

        public void Dispose()
        {
            Socket.Dispose();
        }

        // private void Debug(string msg) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");
    }
}