using Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient;
using Annium.AspNetCore.TestServer;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

public abstract class IntegrationTestBase : IntegrationTest
{
    protected IWebApplicationFactory AppFactory => GetAppFactory<Startup>(
        builder => builder.UseServicePack<ServicePack>(),
        container => container
            .AddTestServerTestClient(x => x
                .WithActiveKeepAlive(600)
                .WithResponseTimeout(6000)
            )
    );
}