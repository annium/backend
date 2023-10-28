using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting.Internal;

internal class TestWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    private readonly Action<IHostBuilder> _configureHost;

    public TestWebApplicationFactory(Action<IHostBuilder> configureHost)
    {
        _configureHost = configureHost;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _configureHost(builder);
        var host = builder.Build();
        host.Start();
        return host;
    }
}
