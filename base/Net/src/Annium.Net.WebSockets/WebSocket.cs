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
            options
        )
        {
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            try
            {
                if (
                    Socket.State == WebSocketState.Connecting ||
                    Socket.State == WebSocketState.Open
                )
                {
                    this.Trace(() => "Disconnect");
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", token);
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

        protected override async Task OnDisconnectAsync()
        {
            this.Trace(() => "Invoke ConnectionLost");
            await ConnectionLost.Invoke();
        }
    }
}