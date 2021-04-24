using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Net.WebSockets.Internal
{
    internal class KeepAliveMonitor : IKeepAliveMonitor
    {
        public CancellationToken Token => _cts.Token;
        private readonly IObservable<SocketMessage> _observable;
        private readonly Func<ReadOnlyMemory<byte>, IObservable<Unit>> _send;
        private readonly ActiveKeepAlive _options;
        private AsyncDisposableBox _disposable = Disposable.AsyncBox();
        private CancellationTokenSource _cts = new();
        private Instant _lastPongTime;

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
            _disposable = Disposable.AsyncBox();

            _disposable += _cts = new();

            var timerInterval = _options.PingInterval.ToTimeSpan();

            // send initial ping within half of interval
            Task.Run(async () =>
            {
                await Task.Delay(timerInterval / 2, CancellationToken.None);
                if (!Token.IsCancellationRequested)
                    SendPing();
            }, CancellationToken.None);

            // run send pings & check pongs on timer
            _disposable += new Timer(SendPingCheckPong, null, timerInterval, timerInterval) as IAsyncDisposable;

            // track pongs
            _disposable += _observable
                .Where(x => x.Type == WebSocketMessageType.Binary && x.Data.Span.SequenceEqual(_options.PongFrame.Span))
                .Subscribe(TrackPong);
        }

        public void Pause()
        {
            _disposable.DisposeAsync().Await();
        }

        private void SendPingCheckPong(object? _)
        {
            // if already canceled - no action
            if (Token.IsCancellationRequested)
                return;

            SendPing();

            // if any ping not responded - signal connection lost
            var now = GetNow();
            if (now - _lastPongTime > _options.PingInterval)
            {
                this.Trace(() => "Missed ping - signal connection lost");
                _cts.Cancel();
            }
        }

        private void SendPing()
        {
            // send ping every time
            this.Trace(() => "Send ping");
            _send(_options.PingFrame).Subscribe();
        }

        private void TrackPong(SocketMessage _)
        {
            this.Trace(() => "Received pong");
            _lastPongTime = GetNow();
        }

        private Instant GetNow() => SystemClock.Instance.GetCurrentInstant();

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}