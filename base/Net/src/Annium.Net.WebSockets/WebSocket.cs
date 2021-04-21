using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;

        public WebSocket(
            NativeWebSocket socket
        ) : base(
            socket
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
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
            }
            catch (WebSocketException)
            {
            }
            finally
            {
                Socket.Dispose();
            }
        }

        protected override async Task OnDisconnectAsync()
        {
            await ConnectionLost.Invoke();
        }
    }
}