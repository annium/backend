using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Internal;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets
{
    public class ClientWebSocket : WebSocketBase<NativeClientWebSocket>, IClientWebSocket
    {
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly ClientWebSocketOptions _options;
        private Uri? _uri;
        private bool _isManuallyDisconnected;

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
            this.Trace(() => $"Connect to {uri}");

            _uri = uri;
            do
            {
                try
                {
                    Socket = new NativeClientWebSocket();
                    this.Trace(() => "Try connect");
                    await Socket.ConnectAsync(uri, token);
                }
                catch (WebSocketException)
                {
                    this.Trace(() => "Connection failed");
                    Socket.Dispose();
                    await Task.Delay(100, token);
                }
            } while (
                !token.IsCancellationRequested &&
                !(
                    Socket.State == WebSocketState.Open ||
                    Socket.State == WebSocketState.CloseSent
                )
            );

            this.Trace(() => "Connected");
            _isManuallyDisconnected = false;
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            _isManuallyDisconnected = true;
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

            if (_isManuallyDisconnected)
            {
                this.Trace(() => "Manually disconnected, no reconnect");
                return;
            }

            if (_options.ReconnectOnFailure)
            {
                this.Trace(() => "Try reconnect");
                await Task.Delay(100);
                await ConnectAsync(_uri!, CancellationToken.None);
                await ConnectionRestored.Invoke();
            }
            else
                this.Trace(() => "No reconnect");
        }
    }
}