using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Logging.Abstractions;
using NodaTime;

namespace Annium.Net.WebSockets.Internal;

internal class KeepAliveMonitor : IKeepAliveMonitor, ILogSubject<KeepAliveMonitor>
{
    public CancellationToken Token => _cts.Token;
    private readonly IObservable<SocketMessage> _observable;
    private readonly Func<ReadOnlyMemory<byte>, IObservable<Unit>> _send;
    private readonly ActiveKeepAlive _options;
    public ILogger<KeepAliveMonitor> Logger { get; }
    private DisposableBox _disposable = Disposable.Box();
    private CancellationTokenSource _cts = new();
    private Instant _lastPongTime;
    private bool _pingStep = true;

    public KeepAliveMonitor(
        IObservable<SocketMessage> observable,
        Func<ReadOnlyMemory<byte>, IObservable<Unit>> send,
        ActiveKeepAlive options,
        ILogger<KeepAliveMonitor> logger
    )
    {
        _observable = observable;
        _send = send;
        _options = options;
        Logger = logger;
    }

    public void Resume()
    {
        this.Log().Trace("start");
        _disposable = Disposable.Box();

        _disposable += _cts = new();

        _lastPongTime = GetNow();

        // run send pings & check pongs on timer
        var timerInterval = _options.PingInterval.ToTimeSpan() / 2;
        _disposable += new Timer(SendPingCheckPong, null, timerInterval, timerInterval);

        // track pongs
        _disposable += _observable
            .Where(x => x.Type == WebSocketMessageType.Binary && x.Data.Span.SequenceEqual(_options.PongFrame.Span))
            .Subscribe(TrackPong);
        this.Log().Trace("done");
    }

    public void Pause()
    {
        this.Log().Trace("start");
        _cts.Cancel();
        _disposable.Dispose();
        this.Log().Trace("done");
    }

    private void SendPingCheckPong(object? _)
    {
        // if already canceled - no action
        if (Token.IsCancellationRequested)
            return;

        if (_pingStep)
            SendPing();
        else
            CheckPong();

        // switch step
        _pingStep = !_pingStep;
    }

    private void SendPing()
    {
        // send ping every time
        this.Log().Trace("Send ping");
        _send(_options.PingFrame).Subscribe();
    }

    private void CheckPong()
    {
        // if any ping not responded - signal connection lost
        var now = GetNow();
        var silenceDuration = now - _lastPongTime;

        // no action, if silence duration is in expected range
        if (silenceDuration <= _options.PingInterval)
            return;

        this.Log().Trace($"Missed ping {Math.Floor(silenceDuration / _options.PingInterval):F0}/{_options.Retries}");
        if (silenceDuration > _options.PingInterval * _options.Retries)
        {
            this.Log().Trace("Missed all pings - signal connection lost");
            _cts.Cancel();
        }
    }

    private void TrackPong(SocketMessage _)
    {
        this.Log().Trace("Received pong");
        _lastPongTime = GetNow();
    }

    private Instant GetNow() => SystemClock.Instance.GetCurrentInstant();
}