using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocketBase<NativeClientWebSocket>, IClientWebSocket
    {
        private readonly ClientWebSocketOptions _options;
        private Uri? _uri;

        public ClientWebSocket(
            ClientWebSocketOptions? options = null
        ) : base(
            new NativeClientWebSocket()
        )
        {
            _options = options ?? new ClientWebSocketOptions
            {
                ReconnectOnFailure = true,
            };
        }

        public async Task ConnectAsync(Uri uri, CancellationToken token)
        {
            _uri = uri;
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
                    await Task.Delay(10, token);
                }
            } while (
                !token.IsCancellationRequested &&
                !(
                    Socket.State == WebSocketState.Open ||
                    Socket.State == WebSocketState.CloseSent
                )
            );
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }

        protected override async Task OnDisconnectAsync()
        {
            if (_options.OnConnectionLost is not null)
                await _options.OnConnectionLost();
            if (_options.ReconnectOnFailure)
            {
                await Task.Delay(10);
                await ConnectAsync(_uri!, CancellationToken.None);
                if (_options.OnConnectionRestored is not null)
                    await _options.OnConnectionRestored();
            }
        }
    }
}