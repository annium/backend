using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient;
using Annium.AspNetCore.TestServer;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public abstract class IntegrationTestBase : IntegrationTest
{
    protected IWebApplicationFactory AppFactory => GetAppFactory<Program>(
        builder => builder.UseServicePack<TestServicePack>(),
        container => container
            .AddTestServerTestClient(x => x
                .WithActiveKeepAlive(600)
                .WithResponseTimeout(6000)
            )
    );
}