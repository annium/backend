using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets.Obsolete;

namespace Annium.Infrastructure.WebSockets.Server;

public class ServerConfiguration
{
    public string PathMatch { get; private set; } = string.Empty;
    public SerializationFormat Format { get; private set; }

    public WebSocketOptions WebSocketOptions { get; private set; } = new()
    {
        ActiveKeepAlive = ActiveKeepAlive.Create(),
        PassiveKeepAlive = PassiveKeepAlive.Create()
    };

    public ServerConfiguration ListenOn(string pathMatch)
    {
        PathMatch = pathMatch;

        return this;
    }

    public ServerConfiguration UseFormat(SerializationFormat format)
    {
        Format = format;

        return this;
    }

    public ServerConfiguration WithActiveKeepAlive(uint pingInterval = 60, uint retries = 3)
    {
        WebSocketOptions.ActiveKeepAlive = ActiveKeepAlive.Create(pingInterval, retries);

        return this;
    }

    public ServerConfiguration UseBinaryFormat()
    {
        Format = SerializationFormat.Binary;

        return this;
    }

    public ServerConfiguration UseTextFormat()
    {
        Format = SerializationFormat.Text;

        return this;
    }
}