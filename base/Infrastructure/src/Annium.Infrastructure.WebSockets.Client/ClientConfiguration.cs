using System;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Net.WebSockets;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client;

public class ClientConfiguration : ClientConfigurationBase<ClientConfiguration>, IClientConfiguration
{
    public Uri Uri { get; private set; } = default!;

    public bool AutoConnect { get; private set; }

    public ClientWebSocketOptions WebSocketOptions { get; } = new()
    {
        ReconnectTimeout = Duration.FromSeconds(5),
        ActiveKeepAlive = ActiveKeepAlive.Create(),
        PassiveKeepAlive = PassiveKeepAlive.Create()
    };

    public ClientConfiguration ConnectTo(Uri uri)
    {
        Uri = uri;

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

    public ClientConfiguration WithActiveKeepAlive(uint pingInterval = 60, uint retries = 5)
    {
        WebSocketOptions.ActiveKeepAlive = ActiveKeepAlive.Create(pingInterval, retries);

        return this;
    }
}

public class TestClientConfiguration : ClientConfigurationBase<TestClientConfiguration>, ITestClientConfiguration
{
    public WebSocketOptions WebSocketOptions { get; } = new()
    {
        ActiveKeepAlive = ActiveKeepAlive.Create(),
        PassiveKeepAlive = PassiveKeepAlive.Create()
    };

    public TestClientConfiguration WithActiveKeepAlive(uint pingInterval = 60, uint retries = 5)
    {
        WebSocketOptions.ActiveKeepAlive = ActiveKeepAlive.Create(pingInterval, retries);

        return this;
    }
}

public abstract class ClientConfigurationBase<TConfiguration> : IClientConfigurationBase
    where TConfiguration : ClientConfigurationBase<TConfiguration>
{
    public SerializationFormat Format { get; private set; }

    public Duration ResponseTimeout { get; private set; } = Duration.FromMinutes(1);

    public TConfiguration UseBinaryFormat()
    {
        Format = SerializationFormat.Binary;

        return (TConfiguration) this;
    }

    public TConfiguration UseTextFormat()
    {
        Format = SerializationFormat.Text;

        return (TConfiguration) this;
    }

    public TConfiguration WithResponseTimeout(uint timeout)
    {
        ResponseTimeout = Duration.FromSeconds(timeout);

        return (TConfiguration) this;
    }
}