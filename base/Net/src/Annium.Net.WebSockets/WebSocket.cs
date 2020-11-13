using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
    {
        public WebSocket(
            NativeWebSocket socket
        ) : base(
            socket
        )
        {
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }

        protected override async Task OnDisconnectAsync()
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
        }
    }
}