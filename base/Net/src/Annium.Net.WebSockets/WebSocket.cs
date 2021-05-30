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
        private bool _isManuallyDisconnected;

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

        public async Task DisconnectAsync(CancellationToken ct)
        {
            _isManuallyDisconnected = true;

            // cancel receive, if pending
            ReceiveCts.Cancel();

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
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", ct);
                }
                else
                    this.Trace(() => "Already disconnected");
            }
            catch (WebSocketException)
            {
                this.Trace(() => nameof(WebSocketException));
            }
            finally
            {
                this.Trace(() => "Dispose socket");
                Socket.Dispose();
            }
        }

        protected override Task OnDisconnectAsync()
        {
            if (_isManuallyDisconnected)
            {
                this.Trace(() => "Manually disconnected, no reconnect");
                return Task.CompletedTask;
            }

            this.Trace(() => "Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

            return Task.CompletedTask;
        }
    }
}