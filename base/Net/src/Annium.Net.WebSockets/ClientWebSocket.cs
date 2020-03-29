using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocketBase<NativeClientWebSocket>, IClientWebSocket
    {
        public ClientWebSocket(
            ISerializer<byte[]> serializer
        ) : base(
            new NativeClientWebSocket(),
            serializer
        )
        {
        }

        public async Task ConnectAsync(Uri uri, CancellationToken token)
        {
            await Socket.ConnectAsync(uri, token);
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