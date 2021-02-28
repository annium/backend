using System;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client
{
    public class ClientConfiguration
    {
        public Uri Uri { get; }
        public bool AutoConnect { get; }
        public bool AutoReconnect { get; }
        public Duration ResponseLifetime { get; }

        public ClientConfiguration(
            Uri uri,
            bool autoConnect,
            bool autoReconnect,
            Duration responseLifetime
        )
        {
            Uri = uri;
            AutoConnect = autoConnect;
            AutoReconnect = autoReconnect;
            ResponseLifetime = responseLifetime;
        }
    }
}