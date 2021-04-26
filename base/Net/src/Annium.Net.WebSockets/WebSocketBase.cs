using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Extensions.Execution;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    // TODO: rewrite as generic socket wrapper
    public abstract class WebSocketBase<TNativeSocket> : ISendingReceivingWebSocket
        where TNativeSocket : NativeWebSocket
    {
        private const int BufferSize = 65536;

        public WebSocketState State => Socket.State;

        protected TNativeSocket Socket { get; set; }
        protected IBackgroundExecutor Executor { get; } = Extensions.Execution.Executor.Background.Sequential();
        protected CancellationTokenSource ReceiveCts { get; private set; } = new();
        private readonly UTF8Encoding _encoding = new();
        private readonly IObservable<SocketMessage> _observable;
        private readonly IObservable<string> _textObservable;
        private readonly IObservable<ReadOnlyMemory<byte>> _binaryObservable;
        private readonly IKeepAliveMonitor _keepAliveMonitor;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        internal WebSocketBase(
            TNativeSocket socket,
            WebSocketBaseOptions options
        )
        {
            Socket = socket;
            _disposable += Socket;
            this.Trace(options.ToString);

            // start socket observable
            var observableInstance = CreateSocketObservable();
            _disposable += observableInstance;

            // resolve components from configuration
            var cfg = Configurator.GetConfiguration(
                observableInstance,
                _encoding,
                x => IsConnected() ? Send(x) : Observable.Empty<Unit>(),
                options
            );
            _keepAliveMonitor = cfg.KeepAliveMonitor;
            _observable = cfg.MessageObservable;
            _binaryObservable = cfg.BinaryObservable;
            _textObservable = cfg.TextObservable;
            _disposable += cfg.Disposable;
        }

        public IObservable<Unit> Send(string data, CancellationToken token = default) =>
            Send(_encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public IObservable<Unit> Send(ReadOnlyMemory<byte> data, CancellationToken token = default) =>
            Send(data, WebSocketMessageType.Binary, token);

        public IObservable<SocketMessage> Listen() => _observable;

        public IObservable<string> ListenText() => _textObservable;

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() => _binaryObservable;

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
                this.Trace(() => "Start, rent buffer");
                var pool = ArrayPool<byte>.Shared;
                var buffer = pool.Rent(BufferSize);

                try
                {
                    this.Trace(() => "Cycle start");

                    // initial spin, until connected
                    Log.Trace(() => "Spin until connected and keepAlive monitor up");
                    SpinWait.SpinUntil(() => _keepAliveMonitor is not null! && IsConnected());
                    _keepAliveMonitor.Resume();
                    ReceiveCts = CancellationTokenSource.CreateLinkedTokenSource(_keepAliveMonitor.Token);

                    // run polling
                    while (!ctx.Token.IsCancellationRequested)
                    {
                        // keep receiving until closed
                        if (await ReceiveAsync(ctx, buffer) == Status.Closed)
                        {
                            this.Trace(() => "Receive ended with Status.Closed - break receive cycle");
                            break;
                        }
                    }

                    // either ct canceled or cycle break due to socket closed
                    this.Trace(() => $"Cycle end: {(ctx.Token.IsCancellationRequested ? "canceled" : "closed")}");
                }
                catch (OperationCanceledException)
                {
                    this.Trace(() => "Operation canceled exception");
                }
                catch (Exception e)
                {
                    this.Trace(() => $"Exception {e}");
                }

                this.Trace(() => "End, return buffer, send OnCompleted");
                pool.Return(buffer);
                ctx.OnCompleted();
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
                    result = await Socket.ReceiveAsync(buffer, ReceiveCts.Token);
                }
                //  remote party closed connection w/o handshake
                catch (WebSocketException e)
                {
                    this.Trace(() => $"{nameof(WebSocketException)} {e}");
                    if (await HandleDisconnect() == Status.Closed)
                        return Status.Closed;
                }
                // token was canceled, no message received - will retry
                catch (OperationCanceledException)
                {
                    this.Trace(() => $"{nameof(OperationCanceledException)} by keepAlive");
                    if (await HandleDisconnect() == Status.Closed)
                        return Status.Closed;
                }
                catch (Exception e)
                {
                    this.Trace(() => $"Exception {e}");
                    // unexpected case
                    throw;
                }

                // if closing - handle disconnect
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    this.Trace(() => "Received close message");
                    if (await HandleDisconnect() == Status.Closed)
                        return Status.Closed;
                }

                stream.Write(buffer[..result.Count].Span);
            } while (!result.EndOfMessage);

            ctx.OnNext(new SocketMessage(result.MessageType, stream.ToArray()));

            return Status.Opened;
        }

        private async Task<Status> HandleDisconnect()
        {
            _keepAliveMonitor.Pause();
            this.Trace(() => "OnDisconnectAsync - start");
            await OnDisconnectAsync().ConfigureAwait(false);
            this.Trace(() => "OnDisconnectAsync - complete");

            // if after disconnect handling not connected - set completed and break
            if (
                State == WebSocketState.CloseReceived ||
                State == WebSocketState.Closed ||
                State == WebSocketState.Aborted
            )
            {
                this.Trace(() => "Closed");
                return Status.Closed;
            }

            _keepAliveMonitor.Resume();
            ReceiveCts = CancellationTokenSource.CreateLinkedTokenSource(_keepAliveMonitor.Token);

            this.Trace(() => "Opened");
            return Status.Opened;
        }

        private bool IsConnected() => State == WebSocketState.Open || State == WebSocketState.CloseSent;

        public async ValueTask DisposeAsync()
        {
            this.Trace(() => "start");
            await _disposable.DisposeAsync();
            this.Trace(() => "done");
        }

        private enum Status
        {
            Opened,
            Closed
        }
    }
}