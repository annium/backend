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
        ) : this(new ClientWebSocketOptions())
        {
        }

        public Task ConnectAsync(Uri uri, CancellationToken token) =>
            ConnectAsync(uri, _options.ConnectTimeout, token);

        public async Task DisconnectAsync(CancellationToken token)
        {
            _isManuallyDisconnected = true;

            // cancel receive, if pending
            ReceiveCts.Cancel();

            this.Trace(() => "Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

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
            if (_isManuallyDisconnected)
            {
                this.Trace(() => "Manually disconnected, no reconnect");
                return;
            }

            this.Trace(() => "Invoke ConnectionLost");
            Executor.Schedule(() => ConnectionLost.Invoke());

            if (_options.ReconnectTimeout != TimeSpan.MaxValue)
            {
                this.Trace(() => "Try reconnect");
                await Task.Delay(_options.ReconnectTimeout);
                await ConnectAsync(_uri!, _options.ReconnectTimeout, CancellationToken.None);
            }
            else
                this.Trace(() => "No reconnect");
        }

        private async Task ConnectAsync(Uri uri, TimeSpan timeout, CancellationToken token)
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
                    await Task.Delay(timeout, token);
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

            this.Trace(() => "Invoke ConnectionRestored");
            Executor.Schedule(() => ConnectionRestored.Invoke());
        }
    }
}