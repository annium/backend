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
        private readonly UTF8Encoding _encoding = new();
        private readonly IObservableInstance<SocketMessage> _socketObservable;

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

        private IObservableInstance<SocketMessage> CreateSocketObservable() =>
            ObservableInstance.Static<SocketMessage>(async ctx =>
            {
                var pool = ArrayPool<byte>.Shared;
                var buffer = pool.Rent(BufferSize);

                try
                {
                    // Debug($"INTERNAL: Resume in {Socket.State} state");

                    // initial spin, until connected
                    SpinWait.SpinUntil(() => State == WebSocketState.Open || State == WebSocketState.CloseSent);

                    while (!ctx.Token.IsCancellationRequested)
                    {
                        // keep receiving until closed
                        if (await ReceiveAsync(ctx, buffer) == Status.Closed)
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    pool.Return(buffer);
                    ctx.OnCompleted();

                    // Debug($"INTERNAL: Suspend in {Socket.State} state");
                }
            });

        private async ValueTask<Status> ReceiveAsync(
            ObserverContext<SocketMessage> ctx,
            Memory<byte> buffer
        )
        {
            await using var stream = new MemoryStream();
            ValueWebSocketReceiveResult result = default;
            do
            {
                try
                {
                    // Debug($"INTERNAL: Receive in State {Socket.State}");
                    result = await Socket.ReceiveAsync(buffer, new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
                    // Debug($"INTERNAL: Received {result.MessageType} in State {Socket.State}");
                }
                //  remote party closed connection w/o handshake
                catch (WebSocketException)
                {
                    // Debug($"INTERNAL: WebSocketException {e}");
                    if (await HandleDisconnect() == Status.Closed)
                        return Status.Closed;
                }
                // token was canceled, no message received - will retry
                catch (OperationCanceledException)
                {
                    return Status.Opened;
                }
                catch (Exception e)
                {
                    // Debug($"INTERNAL: Exception {e}");
                    ctx.OnError(e);
                }

                // if closing - handle disconnect
                if (result.MessageType == WebSocketMessageType.Close && await HandleDisconnect() == Status.Closed)
                    return Status.Closed;

                stream.Write(buffer.Slice(0, result.Count).Span);
            } while (!result.EndOfMessage);

            ctx.OnNext(new SocketMessage(result.MessageType, stream.ToArray()));

            return Status.Opened;

            async Task<Status> HandleDisconnect()
            {
                await OnDisconnectAsync().ConfigureAwait(false);

                // if after disconnect handling not connected - set completed and break
                if (
                    State == WebSocketState.CloseReceived ||
                    State == WebSocketState.Closed ||
                    State == WebSocketState.Aborted
                )
                {
                    return Status.Closed;
                }

                return Status.Opened;
            }
        }

        public ValueTask DisposeAsync()
        {
            Socket.Dispose();
            return _socketObservable.DisposeAsync();
        }

        // private void Debug(string msg) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");

        private enum Status
        {
            Opened,
            Closed
        }
    }
}