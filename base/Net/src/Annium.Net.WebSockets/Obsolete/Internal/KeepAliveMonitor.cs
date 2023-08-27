using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Annium.Debug;
using NodaTime;

namespace Annium.Net.WebSockets.Obsolete.Internal;

[Obsolete]
internal class KeepAliveMonitor : IKeepAliveMonitor
{
    public CancellationToken Token => _cts.Token;
    private readonly IObservable<SocketMessage> _observable;
    private readonly Func<ReadOnlyMemory<byte>, IObservable<Unit>> _send;
    private readonly ActiveKeepAlive _options;
    private DisposableBox _disposable = Disposable.Box();
    private CancellationTokenSource _cts = new();
    private Instant _lastPongTime;
    private bool _pingStep = true;

    public KeepAliveMonitor(
        IObservable<SocketMessage> observable,
        Func<ReadOnlyMemory<byte>, IObservable<Unit>> send,
        ActiveKeepAlive options
    )
    {
        _observable = observable;
        _send = send;
        _options = options;
    }

    public void Resume()
    {
        this.TraceOld("start");
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
        this.TraceOld("done");
    }

    public void Pause()
    {
        this.TraceOld("start");
        _cts.Cancel();
        _disposable.Dispose();
        this.TraceOld("done");
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
        this.TraceOld("Send ping");
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

        this.TraceOld($"Missed ping {Math.Floor(silenceDuration / _options.PingInterval):F0}/{_options.Retries}");
        if (silenceDuration > _options.PingInterval * _options.Retries)
        {
            this.TraceOld("Missed all pings - signal connection lost");
            _cts.Cancel();
        }
    }

    private void TrackPong(SocketMessage _)
    {
        this.TraceOld("Received pong");
        _lastPongTime = GetNow();
    }

    private Instant GetNow() => SystemClock.Instance.GetCurrentInstant();
}