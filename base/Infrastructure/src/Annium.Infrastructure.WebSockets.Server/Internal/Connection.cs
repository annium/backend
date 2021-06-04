using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Net.WebSockets;
using WebSocket = Annium.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Connection : IAsyncDisposable
    {
        public Guid Id { get; }
        public ISendingReceivingWebSocket Socket => _socket;
        private readonly WebSocket _socket;

        public Connection(
            Guid id,
            WebSocket socket
        )
        {
            Id = id;
            _socket = socket;
        }

        public async ValueTask DisposeAsync()
        {
            this.Trace(() => "start");
            if (
                _socket.State == WebSocketState.Connecting ||
                _socket.State == WebSocketState.Open ||
                _socket.State == WebSocketState.CloseReceived
            )
            {
                this.Trace(() => "disconnect socket");
                await _socket.DisconnectAsync();
            }

            this.Trace(() => "dispose socket");
            await _socket.DisposeAsync();
            this.Trace(() => "done");
        }
    }
}