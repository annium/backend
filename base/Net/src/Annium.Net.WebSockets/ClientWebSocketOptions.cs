using System;

namespace Annium.Net.WebSockets
{
    public record ClientWebSocketOptions : WebSocketBaseOptions
    {
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ReconnectTimeout { get; set; } = TimeSpan.MaxValue;
    }
}