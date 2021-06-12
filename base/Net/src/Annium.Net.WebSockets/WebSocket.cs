using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;

        public WebSocket(
            NativeWebSocket socket,
            ILogger<WebSocket> logger
        ) : this(
            socket,
            new WebSocketOptions(),
            logger
        )
        {
        }

        public WebSocket(
            NativeWebSocket socket,
            WebSocketOptions options,
            ILogger<WebSocket> logger
        ) : base(
            socket,
            options,
            Extensions.Execution.Executor.Background.Parallel<WebSocket>(),
            logger
        )
        {
        }

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

        protected override Task OnConnectionLostAsync()
        {
            Logger.Trace("Invoke ConnectionLost");
            Executor.TrySchedule(() => ConnectionLost.Invoke());

            return Task.CompletedTask;
        }

        public override async ValueTask DisposeAsync()
        {
            Logger.Trace("Invoke ConnectionLost");
            Executor.TrySchedule(() => ConnectionLost.Invoke());
            await DisposeBaseAsync();
        }
    }
}