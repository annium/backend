using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    // TODO: rewrite as generic socket wrapper
    public abstract class WebSocketBase<TNativeSocket> : ISendingReceivingWebSocket, ILogSubject
        where TNativeSocket : NativeWebSocket
    {
        private const int BufferSize = 65536;

        public WebSocketState State => Socket.State;
        public ILogger Logger { get; }
        protected TNativeSocket Socket { get; set; }
        protected IBackgroundExecutor Executor { get; }
        private TaskCompletionSource<object> _socketTcs = new();
        private CancellationTokenSource _receiveCts = new();
        private bool IsConnected => State is WebSocketState.Open or WebSocketState.CloseSent;
        private readonly UTF8Encoding _encoding = new();
        private readonly IObservable<SocketMessage> _observable;
        private readonly IObservable<string> _textObservable;
        private readonly IObservable<ReadOnlyMemory<byte>> _binaryObservable;
        private readonly IKeepAliveMonitor _keepAliveMonitor;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        internal WebSocketBase(
            TNativeSocket socket,
            WebSocketBaseOptions options,
            IBackgroundExecutor executor,
            ILogger logger
        )
        {
            Socket = socket;

            Executor = executor;
            Logger = logger;
            Executor.Start();

            // start socket observable
            var observableInstance = CreateSocketObservable();
            _disposable += observableInstance;

            // resolve components from configuration
            this.Log().Trace(options.ToString());
            var cfg = Configurator.GetConfiguration(observableInstance.ObserveOn(TaskPoolScheduler.Default), _encoding, TrySend, options, Logger);
            _keepAliveMonitor = cfg.KeepAliveMonitor;
            _observable = cfg.MessageObservable;
            _binaryObservable = cfg.BinaryObservable;
            _textObservable = cfg.TextObservable;
            _disposable += cfg.Disposable;
        }

        public IObservable<Unit> Send(string data, CancellationToken ct = default) =>
            Send(_encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, ct);

        public IObservable<Unit> Send(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
            Send(data, WebSocketMessageType.Binary, ct);

        public IObservable<SocketMessage> Listen() => _observable;

        public IObservable<string> ListenText() => _textObservable;

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() => _binaryObservable;

        protected void ResumeObservable()
        {
            this.Log().Trace("start");
            _socketTcs.SetResult(default!);
            this.Log().Trace("done");
        }

        protected void PauseObservable()
        {
            if (_receiveCts.IsCancellationRequested)
                this.Log().Trace("receive cts is already disposed");
            else
            {
                this.Log().Trace("cancel and dispose receive cts");
                _receiveCts.Cancel();
                _receiveCts.Dispose();
                this.Log().Trace("canceled and disposed receive cts");
            }
        }

        protected abstract Task OnConnectionLostAsync();

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

        private IAsyncDisposableObservable<SocketMessage> CreateSocketObservable() =>
            ObservableExt.StaticSyncInstance<SocketMessage>(async ctx =>
            {
                this.Log().Trace("register socket tcs completion on observable disposal");
                await using var ctRegistration = ctx.Ct.Register(() =>
                {
                    if (_socketTcs.Task.IsCompleted)
                        this.Log().Trace("observable disposing - socket tcs already released");
                    else
                    {
                        this.Log().Trace("observable disposing - release socket tcs");
                        _socketTcs.SetResult(default!);
                    }
                });
                this.Log().Trace("spin until keepAlive monitor is ready");
                await Wait.UntilAsync(() => _keepAliveMonitor is not null!, CancellationToken.None, pollDelay: 5);

                while (true)
                {
                    this.Log().Trace("wait until ready to receive data from socket");
                    await _socketTcs.Task;

                    if (ctx.Ct.IsCancellationRequested)
                    {
                        this.Log().Trace("disposing, break");
                        break;
                    }

                    this.Log().Trace("start, rent buffer");
                    var pool = ArrayPool<byte>.Shared;
                    var buffer = pool.Rent(BufferSize);

                    try
                    {
                        this.Log().Trace("resume keepAlive monitor");
                        _keepAliveMonitor.Resume();

                        this.Log().Trace("create receive cts, bound to keepAliveMonitor token");
                        _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(_keepAliveMonitor.Token);

                        // run polling
                        this.Log().Trace("start polling");
                        while (true)
                        {
                            // keep receiving while Opened
                            if (await ReceiveAsync(ctx, buffer) == Status.Opened)
                                continue;

                            this.Log().Trace("receive ended with Status.Closed - break receive cycle");
                            break;
                        }

                        // either ct canceled or cycle break due to socket closed
                        this.Log().Trace("end polling");
                    }
                    catch (Exception e)
                    {
                        this.Log().Trace($"exception {e}");
                    }

                    this.Log().Trace("return buffer");
                    pool.Return(buffer);

                    if (ctx.Ct.IsCancellationRequested)
                    {
                        this.Log().Trace("disposing, break");
                        break;
                    }

                    this.Log().Trace("refresh socket tcs");
                    _socketTcs = new TaskCompletionSource<object>();
                }

                this.Log().Trace("send OnCompleted");
                ctx.OnCompleted();

                return () => Task.CompletedTask;
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
                    result = await Socket.ReceiveAsync(buffer, _receiveCts.Token);
                }
                //  remote party closed connection w/o handshake
                catch (WebSocketException e)
                {
                    this.Log().Trace($"{nameof(WebSocketException)} {e}");
                    if (await HandleConnectionLost() == Status.Closed)
                        return Status.Closed;
                }
                // token was canceled, no message received - will retry
                catch (OperationCanceledException)
                {
                    // Operation canceled either by disconnect, or by disposal
                    if (!_keepAliveMonitor.Token.IsCancellationRequested)
                    {
                        this.Log().Trace($"{nameof(OperationCanceledException)} by disconnect or disposal");
                        HandleDisconnected();

                        return Status.Closed;
                    }

                    this.Log().Trace($"{nameof(OperationCanceledException)} by keepAlive");
                    if (await HandleConnectionLost() == Status.Closed)
                        return Status.Closed;
                }
                catch (Exception e)
                {
                    this.Log().Trace($"exception {e}");
                    // unexpected case
                    throw;
                }

                // if closing - handle disconnect
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    this.Log().Trace("received close message");
                    if (await HandleConnectionLost() == Status.Closed)
                        return Status.Closed;
                }

                stream.Write(buffer[..result.Count].Span);
            } while (!result.EndOfMessage);

            ctx.OnNext(new SocketMessage(result.MessageType, stream.ToArray()));

            return Status.Opened;
        }

        private async Task<Status> HandleConnectionLost()
        {
            this.Log().Trace("pause keepAlive");
            _keepAliveMonitor.Pause();

            this.Log().Trace("refresh socket tcs");
            _socketTcs = new TaskCompletionSource<object>();

            this.Log().Trace($"{nameof(OnConnectionLostAsync)} - start in {State} state");
            await OnConnectionLostAsync().ConfigureAwait(false);
            this.Log().Trace($"{nameof(OnConnectionLostAsync)} - complete");

            // if after disconnect handling not connected - set completed and break
            if (State is WebSocketState.CloseReceived or WebSocketState.Closed or WebSocketState.Aborted)
            {
                this.Log().Trace("closed");
                return Status.Closed;
            }

            _keepAliveMonitor.Resume();
            _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(_keepAliveMonitor.Token);

            this.Log().Trace("opened");
            return Status.Opened;
        }

        private void HandleDisconnected()
        {
            this.Log().Trace("pause keepAlive");
            _keepAliveMonitor.Pause();
        }

        private IObservable<Unit> TrySend(ReadOnlyMemory<byte> data)
            => IsConnected ? Send(data) : Observable.Empty<Unit>();

        public abstract ValueTask DisposeAsync();

        protected async ValueTask DisposeBaseAsync()
        {
            this.Log().Trace("pause observable");
            PauseObservable();
            this.Log().Trace("dispose bundle");
            await _disposable.DisposeAsync();
            this.Log().Trace("dispose executor");
            await Executor.DisposeAsync();
            this.Log().Trace("dispose socket");
            await Socket.DisposeAsync();
            this.Log().Trace("done");
        }

        private enum Status
        {
            Opened,
            Closed
        }
    }
}