using System;
using Annium.Net.WebSockets;
using NodaTime;

namespace Annium.Infrastructure.WebSockets.Client;

public class ClientConfiguration : ClientConfigurationBase<ClientConfiguration>, IClientConfiguration
{
    public Uri Uri { get; private set; } = default!;

    public ClientWebSocketOptions WebSocketOptions { get; } = ClientWebSocketOptions.Default;

    public ClientConfiguration ConnectTo(Uri uri)
    {
        Uri = uri;

        return this;
    }
}

public class TestClientConfiguration : ClientConfigurationBase<TestClientConfiguration>, ITestClientConfiguration
{
    public ServerWebSocketOptions WebSocketOptions { get; } = ServerWebSocketOptions.Default;
}

public abstract class ClientConfigurationBase<TConfiguration> : IClientConfigurationBase
    where TConfiguration : ClientConfigurationBase<TConfiguration>
{
    public Duration ResponseTimeout { get; private set; } = Duration.FromMinutes(1);

    public TConfiguration WithResponseTimeout(uint timeout)
    {
        ResponseTimeout = Duration.FromSeconds(timeout);

        return (TConfiguration)this;
    }
}