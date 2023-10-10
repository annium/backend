using System.Net.WebSockets;
using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient;
using Annium.AspNetCore.TestServer;
using Annium.Mesh.Transport.WebSockets;
using Annium.Net.WebSockets;
using Xunit.Abstractions;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public abstract class IntegrationTestBase : IntegrationTest
{
    protected IWebApplicationFactory AppFactory { get; }

    protected IntegrationTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        AppFactory = GetAppFactory<Program>(
            builder => builder.UseServicePack<TestServicePack>(),
            container => container
                .AddMeshWebSocketsClientTransport(_ => new ClientTransportConfiguration
                {
                    ConnectionMonitor = ConnectionMonitor.None
                })
                .AddTestServerManagedClient<WebSocket>(x => x
                    .WithResponseTimeout(6000)
                )
        );
    }
}