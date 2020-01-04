using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Serialization.Abstractions;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocket<NativeClientWebSocket>, IClientWebSocket
    {
        public ClientWebSocket(
            ISerializer<ReadOnlyMemory<byte>> serializer
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
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }
    }
}