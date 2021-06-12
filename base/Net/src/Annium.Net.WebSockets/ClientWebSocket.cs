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
            CancelReceive();

            Logger.Trace("Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

            try
            {
                if (
                    Socket.State == WebSocketState.Connecting ||
                    Socket.State == WebSocketState.Open
                )
                {
                    Logger.Trace("Disconnect");
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
                }
                else
                    Logger.Trace("Already disconnected");
            }
            catch (WebSocketException)
            {
                Logger.Trace(nameof(WebSocketException));
            }
        }

        protected override async Task OnConnectionLostAsync()
        {
            Logger.Trace("Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

            if (_options.ReconnectTimeout != Duration.MaxValue)
            {
                Logger.Trace("Try reconnect");
                await Task.Delay(_options.ReconnectTimeout.ToTimeSpan());
                await ConnectAsync(_uri!, _options.ReconnectTimeout, CancellationToken.None);
            }
            else
                Logger.Trace("No reconnect");
        }

        private async Task ConnectAsync(Uri uri, Duration timeout, CancellationToken ct)
        {
            Logger.Trace($"Connect to {uri}");

            _uri = uri;
            do
            {
                try
                {
                    Socket = new NativeClientWebSocket();
                    Logger.Trace("Try connect");
                    await Socket.ConnectAsync(uri, ct);
                }
                catch (WebSocketException)
                {
                    Logger.Trace("Connection failed");
                    Socket.Dispose();
                    await Task.Delay(timeout.ToTimeSpan(), ct);
                }
            } while (
                !ct.IsCancellationRequested &&
                !(
                    Socket.State == WebSocketState.Open ||
                    Socket.State == WebSocketState.CloseSent
                )
            );

            Logger.Trace("Connected");

            Logger.Trace("Invoke ConnectionRestored");
            Executor.Schedule(() => ConnectionRestored.Invoke());
        }

        public override async ValueTask DisposeAsync()
        {
            Logger.Trace("Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());
            await DisposeBaseAsync();
        }
    }
}