using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using NodaTime;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocketBase<NativeClientWebSocket>, IClientWebSocket
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly ClientWebSocketOptions _options;
        private Uri? _uri;

        public ClientWebSocket(
            ClientWebSocketOptions options,
            ILogger<ClientWebSocket> logger
        ) : base(
            new NativeClientWebSocket(),
            options,
            Extensions.Execution.Executor.Background.Parallel<ClientWebSocket>(),
            logger
        )
        {
            _options = options;
        }

        public ClientWebSocket(
            ILogger<ClientWebSocket> logger
        ) : this(new ClientWebSocketOptions(), logger)
        {
        }

        public Task ConnectAsync(Uri uri, CancellationToken ct) =>
            ConnectAsync(uri, _options.ConnectTimeout, ct);

        public async Task DisconnectAsync()
        {
            // cancel receive, if pending
            PauseObservable();

            this.Log().Trace("invoke ConnectionLost in {state}", Socket.State);
            Executor.Schedule(() => ConnectionLost.Invoke());

            try
            {
                if (
                    Socket.State == WebSocketState.Connecting ||
                    Socket.State == WebSocketState.Open
                )
                {
                    this.Log().Trace("disconnect");
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
                }
                else
                    this.Log().Trace("already disconnected");
            }
            catch (WebSocketException)
            {
                this.Log().Trace(nameof(WebSocketException));
            }
        }

        protected override async Task OnConnectionLostAsync()
        {
            this.Log().Trace("invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

            if (_options.ReconnectTimeout != Duration.MaxValue)
            {
                this.Log().Trace("try reconnect");
                await Task.Delay(_options.ReconnectTimeout.ToTimeSpan());
                await ConnectAsync(_uri!, _options.ReconnectTimeout, CancellationToken.None);
            }
            else
                this.Log().Trace("no reconnect");
        }

        private async Task ConnectAsync(Uri uri, Duration timeout, CancellationToken ct)
        {
            this.Log().Trace($"connect to {uri}");

            _uri = uri;
            do
            {
                try
                {
                    Socket = new NativeClientWebSocket();
                    this.Log().Trace("try connect");
                    await Socket.ConnectAsync(uri, ct);
                }
                catch (WebSocketException)
                {
                    this.Log().Trace("connection failed");
                    Socket.Dispose();
                    await Task.Delay(timeout.ToTimeSpan(), ct);
                }
            } while (
                !ct.IsCancellationRequested &&
                Socket.State is not (WebSocketState.Open or WebSocketState.CloseSent)
            );

            if (Socket.State is WebSocketState.Open or WebSocketState.CloseSent)
            {
                this.Log().Trace("connected");
                ResumeObservable();

                this.Log().Trace("invoke ConnectionRestored");
                Executor.Schedule(() => ConnectionRestored.Invoke());
            }
            else
                this.Log().Trace("connected");
        }

        public override async ValueTask DisposeAsync()
        {
            this.Log().Trace("start in {state}", Socket.State);
            if (Socket.State is WebSocketState.Connecting or WebSocketState.Open)
            {
                this.Log().Trace("invoke ConnectionLost");
                Executor.Schedule(() => ConnectionLost.Invoke());
            }

            await DisposeBaseAsync();

            this.Log().Trace("done");
        }
    }
}