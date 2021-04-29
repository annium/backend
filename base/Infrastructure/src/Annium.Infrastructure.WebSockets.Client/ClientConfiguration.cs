using System;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client
{
    public class ClientConfiguration
    {
        public Uri Uri { get; private set; } = default!;
        public SerializationFormat Format { get; private set; }
        public bool AutoConnect { get; private set; }

        public ClientWebSocketOptions WebSocketOptions { get; private set; } = new()
        {
            ReconnectTimeout = Duration.FromSeconds(5),
            ActiveKeepAlive = ActiveKeepAlive.Create(),
            PassiveKeepAlive = PassiveKeepAlive.Create()
        };

        public Duration ResponseTimeout { get; private set; } = Duration.FromMinutes(1);

        public ClientConfiguration ConnectTo(Uri uri)
        {
            Uri = uri;

            return this;
        }

        public ClientConfiguration UseBinaryFormat()
        {
            Format = SerializationFormat.Binary;

            return this;
        }

        public ClientConfiguration UseTextFormat()
        {
            Format = SerializationFormat.Text;

            return this;
        }

        public ClientConfiguration WithAutoConnect()
        {
            AutoConnect = true;

            return this;
        }

        public ClientConfiguration WithConnectTimeout(uint timeout)
        {
            WebSocketOptions.ConnectTimeout = Duration.FromSeconds(timeout);

            return this;
        }

        public ClientConfiguration WithReconnectTimeout(uint timeout)
        {
            WebSocketOptions.ReconnectTimeout = Duration.FromSeconds(timeout);

            return this;
        }

        public ClientConfiguration WithActiveKeepAlive(uint pingInterval = 60, uint retries = 3)
        {
            WebSocketOptions.ActiveKeepAlive = ActiveKeepAlive.Create(pingInterval, retries);

            return this;
        }

        public ClientConfiguration WithResponseTimeout(uint timeout)
        {
            ResponseTimeout = Duration.FromSeconds(timeout);
            return this;
        }
    }
}