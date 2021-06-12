using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using WebSocket = Annium.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Connection : IAsyncDisposable
    {
        public Guid Id { get; }
        public ISendingReceivingWebSocket Socket => _socket;
        private readonly WebSocket _socket;
        private readonly ILogger<Connection> _logger;

        public Connection(
            Guid id,
            WebSocket socket,
            ILogger<Connection> logger
        )
        {
            Id = id;
            _socket = socket;
            _logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            _logger.Trace("start");
            if (
                _socket.State == WebSocketState.Connecting ||
                _socket.State == WebSocketState.Open ||
                _socket.State == WebSocketState.CloseReceived
            )
            {
                _logger.Trace("disconnect socket");
                await _socket.DisconnectAsync();
            }

            _logger.Trace("dispose socket");
            await _socket.DisposeAsync();
            _logger.Trace("done");
        }
    }
}