using System;
using System.IO;
using Annium.Core.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting;

internal class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly Action<IHostBuilder> _configureHost;

    public TestWebApplicationFactory(Action<IHostBuilder> configureHost)
    {
        _configureHost = configureHost;
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        _configureHost(hostBuilder);

        return hostBuilder
            .ConfigureLoggingBridge()
            .ConfigureWebHostDefaults(builder => builder
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<TStartup>()
            );
    }
}