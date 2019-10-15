using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocket<NativeClientWebSocket>
    {
        public ClientWebSocket(
            MessageFormat format
        ) : base(
            new NativeClientWebSocket(),
            format
        )
        {

        }

        public async Task ConnectAsync(Uri uri, CancellationToken token)
        {
            await socket.ConnectAsync(uri, token);
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }
    }
}