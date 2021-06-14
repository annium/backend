using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;
using WebSocket = Annium.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class Connection : IAsyncDisposable, ILogSubject
    {
        public Guid Id { get; }
        public ISendingReceivingWebSocket Socket => _socket;
        public ILogger Logger { get; }
        private readonly WebSocket _socket;

        public Connection(
            Guid id,
            WebSocket socket,
            ILogger<Connection> logger
        )
        {
            Id = id;
            _socket = socket;
            Logger = logger;
        }

        public async ValueTask DisposeAsync()
        {
            this.Log().Trace("start");
            if (
                _socket.State == WebSocketState.Connecting ||
                _socket.State == WebSocketState.Open ||
                _socket.State == WebSocketState.CloseReceived
            )
            {
                this.Log().Trace("disconnect socket");
                await _socket.DisconnectAsync();
            }

            this.Log().Trace("dispose socket");
            await _socket.DisposeAsync();
            this.Log().Trace("done");
        }
    }
}