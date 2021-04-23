using NodaTime;

namespace Annium.Net.WebSockets
{
    public class WebSocketBaseOptions
    {
        public ActiveKeepAlive? ActiveKeepAlive { get; init; }
        public PassiveKeepAlive? PassiveKeepAlive { get; init; }
    }

    public class ActiveKeepAlive : PassiveKeepAlive
    {
        public static ActiveKeepAlive Create(
            string pingFrame = "ping",
            int pingInterval = 1,
            string pongFrame = "pong"
        ) => new()
        {
            PingFrame = pingFrame,
            PingInterval = Duration.FromMinutes(pingInterval),
            PongFrame = pongFrame,
        };

        public Duration PingInterval { get; init; }
    }

    public class PassiveKeepAlive
    {
        public static PassiveKeepAlive Create(string pingFrame = "ping", string pongFrame = "pong") => new()
        {
            PingFrame = pingFrame,
            PongFrame = pongFrame,
        };

        public string PingFrame { get; init; }
        public string PongFrame { get; init; }
    }
}