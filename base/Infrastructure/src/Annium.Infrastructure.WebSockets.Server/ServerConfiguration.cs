using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server
{
    public class ServerConfiguration
    {
        public string Endpoint { get; private set; } = "/ws";
        public SerializationFormat Format { get; private set; }

        public WebSocketOptions WebSocketOptions { get; private set; } = new()
        {
            ActiveKeepAlive = ActiveKeepAlive.Create(),
            PassiveKeepAlive = PassiveKeepAlive.Create()
        };

        public ServerConfiguration ListenAt(string endpoint)
        {
            Endpoint = endpoint;

            return this;
        }

        public ServerConfiguration UseFormat(SerializationFormat format)
        {
            Format = format;

            return this;
        }

        public ServerConfiguration UseWebSocketConfiguration(WebSocketOptions webSocketOptions)
        {
            WebSocketOptions = webSocketOptions;

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
}