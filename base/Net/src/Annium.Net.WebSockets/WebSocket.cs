using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;

        public WebSocket(
            NativeWebSocket socket
        ) : this(
            socket,
            new WebSocketOptions()
        )
        {
        }

        public WebSocket(
            NativeWebSocket socket,
            WebSocketOptions options
        ) : base(
            socket,
            options,
            Extensions.Execution.Executor.Background.Sequential<WebSocket>()
        )
        {
        }

        public async Task DisconnectAsync()
        {
            // cancel receive, if pending
            CancelReceive();

            this.Trace(() => "Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

            try
            {
                if (
                    Socket.State == WebSocketState.Connecting ||
                    Socket.State == WebSocketState.Open
                )
                {
                    this.Trace(() => "Disconnect");
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", CancellationToken.None);
                }
                else
                    this.Trace(() => "Already disconnected");
            }
            catch (WebSocketException)
            {
                this.Trace(() => nameof(WebSocketException));
            }
        }

        protected override Task OnConnectionLostAsync()
        {
            this.Trace(() => "Invoke ConnectionLost");
            Executor.TrySchedule(() => ConnectionLost.Invoke());

            return Task.CompletedTask;
        }

        public override async ValueTask DisposeAsync()
        {
            this.Trace(() => "Invoke ConnectionLost");
            Executor.TrySchedule(() => ConnectionLost.Invoke());
            await DisposeBaseAsync();
        }
    }
}