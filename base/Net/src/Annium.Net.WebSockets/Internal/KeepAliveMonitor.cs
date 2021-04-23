using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using NodaTime;

namespace Annium.Net.WebSockets.Internal
{
    internal class KeepAliveMonitor : IAsyncDisposable
    {
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();
        private Instant _lastPongTime;

        public KeepAliveMonitor(
            ISendingReceivingWebSocket socket,
            Encoding encoding,
            ActiveKeepAlive options,
            Action signal
        )
        {
            var timerInterval = options.PingInterval.ToTimeSpan();
            _disposable += new Timer(_ =>
            {
                // send ping every time
                socket.Send(options.PingFrame, CancellationToken.None).Subscribe();

                // if any ping not responded - signal connection lost
                if (_lastPongTime < GetNow() - options.PingInterval)
                    signal();
            }, null, timerInterval, timerInterval) as IAsyncDisposable;
            _disposable += socket.Listen()
                .Where(x => x.Type == WebSocketMessageType.Text)
                .Select(x => encoding.GetString(x.Data.Span))
                .Where(x => x == options.PongFrame)
                .Subscribe(_ => _lastPongTime = GetNow());
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        private Instant GetNow() => SystemClock.Instance.GetCurrentInstant();
    }
}