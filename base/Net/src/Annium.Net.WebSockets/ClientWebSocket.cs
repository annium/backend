using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocketBase<NativeClientWebSocket>, IClientWebSocket
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly ClientWebSocketOptions _options;
        private Uri? _uri;

        public ClientWebSocket(
            ClientWebSocketOptions options
        ) : base(
            new NativeClientWebSocket(),
            options
        )
        {
            _options = options;
        }

        public ClientWebSocket(
        ) : this(
            new ClientWebSocketOptions
            {
                ReconnectOnFailure = true,
            }
        )
        {
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
            try
            {
                if (
                    Socket.State == WebSocketState.Connecting ||
                    Socket.State == WebSocketState.Open
                )
                    await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Normal close", token);
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
            if (_options.ReconnectOnFailure)
            {
                await Task.Delay(10);
                await ConnectAsync(_uri!, CancellationToken.None);
                await ConnectionRestored.Invoke();
            }
        }
    }
}