using System;
using System.Net.WebSockets;
using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient.Clients;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Client;

namespace Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient;

public static class TestServerClientServiceContainerExtensions
{
    public static IServiceContainer AddTestServerClient(
        this IServiceContainer container,
        Action<ClientConfiguration> configure
    )
    {
        // client
        container.Add(sp =>
        {
            var configuration = new ClientConfiguration();
            configure(configuration);

            var factory = sp.Resolve<IClientFactory>();
            var client = factory.Create(configuration);

            return new TestServerClient(client);
        }).AsSelf().Singleton();

        // system
        container.AddWebSocketClient();

        return container;
    }

    public static IServiceContainer AddTestServerTestClient(
        this IServiceContainer container,
        Action<TestClientConfiguration>? configure = default
    )
    {
        // test client
        container.Add<Func<WebSocket, TestServerTestClient>>(sp =>
        {
            var configuration = new TestClientConfiguration();
            configure?.Invoke(configuration);

            var factory = sp.Resolve<ITestClientFactory>();

            return socket => new TestServerTestClient(factory.Create(socket, configuration));
        }).AsSelf().Singleton();

        // system
        container.AddWebSocketClient();

        return container;
    }
}