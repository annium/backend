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
        private readonly ClientWebSocketOptions _options;
        private Uri? _uri;

        public ClientWebSocket(
            ISerializer<byte[]> serializer,
            ClientWebSocketOptions? options = null
        ) : base(
            new NativeClientWebSocket(),
            serializer
        )
        {
            this._options = options ?? new ClientWebSocketOptions
            {
                ReconnectOnFailure = true,
            };
        }

        public async Task ConnectAsync(Uri uri, CancellationToken token)
        {
            this._uri = uri;
            do
            {
                try
                {
                    Socket = new NativeClientWebSocket();
                    await Socket.ConnectAsync(uri, token);
                }
                catch (WebSocketException)
                {
                    Socket.Dispose();
                    await Task.Delay(10);
                }
            } while (!IsConnected && !token.IsCancellationRequested);
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }

        protected override async Task OnDisconnectAsync()
        {
            if (!IsConnected)
                throw new InvalidOperationException("WebSocket is not connected");

            if (_options.ReconnectOnFailure)
            {
                await ConnectAsync(_uri!, CancellationToken.None);
                if (_options.AfterReconnect != null)
                    await _options.AfterReconnect();
            }
            else
                await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
        }
    }
}