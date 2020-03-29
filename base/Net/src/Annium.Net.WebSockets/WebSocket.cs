using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public class WebSocket : WebSocketBase<NativeWebSocket>, IWebSocket
    {
        public WebSocket(
            NativeWebSocket socket,
            ISerializer<byte[]> serializer
        ) : base(
            socket,
            serializer
        )
        {
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            if (
                Socket.State == WebSocketState.Open ||
                Socket.State == WebSocketState.CloseReceived ||
                Socket.State == WebSocketState.CloseSent
            )
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }
    }
}