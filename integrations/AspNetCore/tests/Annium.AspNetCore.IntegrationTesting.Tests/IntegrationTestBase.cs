using System.Net.WebSockets;
using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient;
using Annium.AspNetCore.TestServer;
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
                .AddTestServerManagedClient<WebSocket>(x => x
                    .WithResponseTimeout(6000)
                )
        );
    }
}