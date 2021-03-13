using System;
using Annium.Infrastructure.WebSockets.Domain;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client
{
    public class ClientConfiguration
    {
        public Uri Uri { get; private set; }
        public SerializationFormat Format { get; private set; }
        public bool AutoConnect { get; private set; }
        public bool AutoReconnect { get; private set; }
        public Duration ResponseLifetime { get; private set; } = Duration.FromMinutes(1);

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

        public ClientConfiguration WithAutoReconnect()
        {
            AutoReconnect = true;

            return this;
        }

        public ClientConfiguration WithResponseLifetime(Duration responseLifetime)
        {
            ResponseLifetime = responseLifetime;
            return this;
        }
    }
}