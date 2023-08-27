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
using Annium.Debug;
using Annium.Extensions.Execution;
using Annium.Net.WebSockets.Obsolete.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Obsolete;

// TODO: rewrite as generic socket wrapper
[Obsolete]
public abstract class WebSocketBase<TNativeSocket> : ISendingReceivingWebSocket
    where TNativeSocket : NativeWebSocket
{
    private const int BufferSize = 65536;

    public WebSocketState State => Socket.State;
    protected TNativeSocket Socket { get; set; }
    protected IBackgroundExecutor Executor { get; }
    private TaskCompletionSource<object> _socketTcs = new();
    private CancellationTokenSource _receiveCts = new();
    private bool IsConnected => State is WebSocketState.Open or WebSocketState.CloseSent;
    private readonly UTF8Encoding _encoding = new();
    private readonly CancellationTokenSource _observableCts = new();
    private readonly IObservable<SocketMessage> _observable;
    private readonly IObservable<SocketMessage> _messageObservable;
    private readonly IObservable<string> _textObservable;
    private readonly IObservable<ReadOnlyMemory<byte>> _binaryObservable;
    private readonly IKeepAliveMonitor _keepAliveMonitor;
    private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

    internal WebSocketBase(
        TNativeSocket socket,
        IBackgroundExecutor executor,
        WebSocketBaseOptions options,
        WebSocketConfig config
    )
    {
        Socket = socket;
        Executor = executor;

        // start socket observable
        _observable = CreateSocketObservable(_observableCts.Token)
            .TrackCompletion();

        // resolve components from configuration
        var cfg = Configurator.GetConfiguration(_observable.ObserveOn(TaskPoolScheduler.Default), _encoding, TrySend, options);
        _keepAliveMonitor = cfg.KeepAliveMonitor;
        _messageObservable = cfg.MessageObservable;
        _binaryObservable = cfg.BinaryObservable;
        _textObservable = cfg.TextObservable;
        _disposable += cfg.Disposable;

        if (config.ResumeImmediately)
            ResumeObservable();

        Executor.Start();
    }

    public IObservable<Unit> Send(string data, CancellationToken ct = default) =>
        Send(_encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, ct);

    public IObservable<Unit> Send(ReadOnlyMemory<byte> data, CancellationToken ct = default) =>
        Send(data, WebSocketMessageType.Binary, ct);

    public IObservable<SocketMessage> Listen() => _messageObservable;

    public IObservable<string> ListenText() => _textObservable;

    public IObservable<ReadOnlyMemory<byte>> ListenBinary() => _binaryObservable;

    protected void ResumeObservable()
    {
        this.TraceOld("start");
        _socketTcs.SetResult(default!);
        this.TraceOld("done");
    }

    protected void PauseObservable()
    {
        if (_receiveCts.IsCancellationRequested)
            this.TraceOld("receive cts is already disposed");
        else
        {
            this.TraceOld("cancel and dispose receive cts");
            _receiveCts.Cancel();
            _receiveCts.Dispose();
            this.TraceOld("canceled and disposed receive cts");
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

    private IObservable<SocketMessage> CreateSocketObservable(CancellationToken ct) => ObservableExt.StaticSyncInstance<SocketMessage>(async ctx =>
    {
        this.TraceOld("create observable");
        await using var _ = ctx.Ct.Register(() =>
        {
            if (_socketTcs.Task.IsCompleted)
                this.TraceOld("observable disposing - socket tcs already released");
            else
            {
                this.TraceOld("observable disposing - release socket tcs");
                _socketTcs.SetResult(default!);
            }
        });

        while (true)
        {
            await _socketTcs.Task;
            this.TraceOld("start receiving data from socket");

            if (ctx.Ct.IsCancellationRequested)
            {
                this.TraceOld("disposing, break");
                break;
            }

            this.TraceOld("start, rent buffer");
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(BufferSize);

            try
            {
                this.TraceOld("resume keepAlive monitor");
                _keepAliveMonitor.Resume();

                this.TraceOld("create receive cts, bound to keepAliveMonitor token");
                _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(_keepAliveMonitor.Token);

                // run polling
                this.TraceOld("start polling");
                while (true)
                {
                    // keep receiving while Opened
                    if (await ReceiveAsync(ctx, buffer) == Status.Opened)
                        continue;

                    this.TraceOld("receive ended with Status.Closed - break receive cycle");
                    break;
                }

                // either ct canceled or cycle break due to socket closed
                this.TraceOld("end polling");
            }
            catch (Exception e)
            {
                this.TraceOld($"exception {e}");
            }

            this.TraceOld("return buffer");
            pool.Return(buffer);

            if (ctx.Ct.IsCancellationRequested)
            {
                this.TraceOld("disposing, break");
                break;
            }

            this.TraceOld("refresh socket tcs");
            _socketTcs = new TaskCompletionSource<object>();
        }

        this.TraceOld("done");

        return () => Task.CompletedTask;
    }, ct);

    private async ValueTask<Status> ReceiveAsync(
        ObserverContext<SocketMessage> ctx,
        Memory<byte> buffer
    )
    {
        await using var stream = new MemoryStream();
        ValueWebSocketReceiveResult? result;
        do
        {
            result = await ReceiveChunkAsync(buffer);
            if (result is null)
            {
                this.TraceOld("result is empty - assume socket is closed");
                return Status.Closed;
            }

            // if closing - handle disconnect
            if (result.Value.MessageType == WebSocketMessageType.Close)
            {
                this.TraceOld("received close message");
                if (await HandleConnectionLost() == Status.Closed)
                {
                    this.TraceOld("connection closed after connection lost by close message");
                    return Status.Closed;
                }
            }

            stream.Write(buffer[..result.Value.Count].Span);
        }
        while (!result.Value.EndOfMessage);

        ctx.OnNext(new SocketMessage(result.Value.MessageType, stream.ToArray()));

        return Status.Opened;
    }

    private async ValueTask<ValueWebSocketReceiveResult?> ReceiveChunkAsync(
        Memory<byte> buffer
    )
    {
        try
        {
            return await Socket.ReceiveAsync(buffer, _receiveCts.Token);
        }
        //  remote party closed connection w/o handshake
        catch (WebSocketException e)
        {
            this.TraceOld($"{nameof(WebSocketException)} {e}");
            if (await HandleConnectionLost() == Status.Closed)
            {
                this.TraceOld("connection closed after connection lost by WebSocket exception");
                return null;
            }
        }
        // token was canceled, no message received - will retry
        catch (OperationCanceledException)
        {
            // Operation canceled either by disconnect, or by disposal
            if (!_keepAliveMonitor.Token.IsCancellationRequested)
            {
                this.TraceOld($"{nameof(OperationCanceledException)} by disconnect or disposal");
                HandleDisconnected();

                return null;
            }

            this.TraceOld($"{nameof(OperationCanceledException)} by keepAlive");
            if (await HandleConnectionLost() == Status.Closed)
            {
                this.TraceOld("connection closed after connection lost by keepAlive");
                return null;
            }
        }
        catch (Exception e)
        {
            this.TraceOld($"exception {e}");
            // unexpected case
            throw;
        }

        return null;
    }

    private async Task<Status> HandleConnectionLost()
    {
        this.TraceOld("pause keepAlive");
        _keepAliveMonitor.Pause();

        this.TraceOld("refresh socket tcs");
        _socketTcs = new TaskCompletionSource<object>();

        this.TraceOld($"{nameof(OnConnectionLostAsync)} - start in {State} state");
        await OnConnectionLostAsync().ConfigureAwait(false);
        this.TraceOld($"{nameof(OnConnectionLostAsync)} - complete");

        // if after disconnect handling not connected - set completed and break
        if (State is WebSocketState.CloseReceived or WebSocketState.Closed or WebSocketState.Aborted)
        {
            this.TraceOld("closed");
            return Status.Closed;
        }

        _keepAliveMonitor.Resume();
        _receiveCts = CancellationTokenSource.CreateLinkedTokenSource(_keepAliveMonitor.Token);

        this.TraceOld("opened");
        return Status.Opened;
    }

    private void HandleDisconnected()
    {
        this.TraceOld("pause keepAlive");
        _keepAliveMonitor.Pause();
    }

    private IObservable<Unit> TrySend(ReadOnlyMemory<byte> data)
        => IsConnected ? Send(data) : Observable.Empty<Unit>();

    public abstract ValueTask DisposeAsync();

    protected async ValueTask DisposeBaseAsync()
    {
        this.TraceOld("pause observable");
        PauseObservable();
        this.TraceOld("dispose bundle");
        await _disposable.DisposeAsync();
        this.TraceOld("cancel observable cts");
        _observableCts.Cancel();
        this.TraceOld("await observable");
        await _observable.WhenCompleted();
        this.TraceOld("dispose executor");
        await Executor.DisposeAsync();
        this.TraceOld("dispose socket");
        await Socket.DisposeAsync();
        this.TraceOld("done");
    }

    private enum Status
    {
        Opened,
        Closed
    }
}