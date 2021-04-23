using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Net.WebSockets.Internal
{
    internal class KeepAliveMonitor : IAsyncDisposable
    {
        private readonly ISendingReceivingWebSocket _socket;
        private readonly ActiveKeepAlive _options;
        private readonly Action _signal;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();
        private Instant _lastPongTime;

        public KeepAliveMonitor(
            ISendingReceivingWebSocket socket,
            Encoding encoding,
            ActiveKeepAlive options,
            Action signal
        )
        {
            _socket = socket;
            _options = options;
            _signal = signal;

            // send initial ping immediately
            SendPing();

            // run send pings & check pongs on timer
            var timerInterval = options.PingInterval.ToTimeSpan();
            _disposable += new Timer(SendPingCheckPong, null, timerInterval, timerInterval) as IAsyncDisposable;

            // track pongs
            _disposable += socket.Listen()
                .Where(x => x.Type == WebSocketMessageType.Text)
                .Select(x => encoding.GetString(x.Data.Span))
                .Where(x => x == options.PongFrame)
                .Subscribe(TrackPong);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        private void SendPingCheckPong(object? _)
        {
            SendPing();

            // if any ping not responded - signal connection lost
            var now = GetNow();
            if (now - _lastPongTime > _options.PingInterval)
            {
                _socket.Trace(() => $"KeepAlive: missed {_options.PingFrame} - signal connection lost");
                _signal();
            }
        }

        private void SendPing()
        {
            // send ping every time
            _socket.Trace(() => $"KeepAlive: send {_options.PingFrame}");
            _socket.Send(_options.PingFrame, CancellationToken.None).Subscribe();
        }

        private void TrackPong(string _)
        {
            _socket.Trace(() => $"KeepAlive: received {_options.PongFrame}");
            _lastPongTime = GetNow();
        }

        private Instant GetNow() => SystemClock.Instance.GetCurrentInstant();
    }
}