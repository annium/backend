using System;
using NodaTime;

namespace Annium.Net.WebSockets
{
    public record ClientWebSocketOptions : WebSocketBaseOptions
    {
        public Duration ConnectTimeout { get; set; } = Duration.FromSeconds(30);
        public Duration ReconnectTimeout { get; set; } = Duration.MaxValue;
    }
}