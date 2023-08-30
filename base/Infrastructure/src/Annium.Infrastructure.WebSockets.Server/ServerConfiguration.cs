using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server;

public class ServerConfiguration
{
    public string PathMatch { get; private set; } = string.Empty;

    public ServerWebSocketOptions WebSocketOptions { get; private set; } = ServerWebSocketOptions.Default;

    public ServerConfiguration ListenOn(string pathMatch)
    {
        PathMatch = pathMatch;

        return this;
    }
}