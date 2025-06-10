using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting.Internal;

/// <summary>
/// Web application factory for integration testing that allows custom host configuration
/// </summary>
/// <typeparam name="TEntryPoint">The entry point class for the application</typeparam>
internal class TestWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    /// <summary>
    /// The action to configure the host builder
    /// </summary>
    private readonly Action<IHostBuilder> _configureHost;

    /// <summary>
    /// Initializes a new instance of the TestWebApplicationFactory class
    /// </summary>
    /// <param name="configureHost">Action to configure the host builder</param>
    public TestWebApplicationFactory(Action<IHostBuilder> configureHost)
    {
        _configureHost = configureHost;
    }

    /// <summary>
    /// Creates and configures the host for the web application
    /// </summary>
    /// <param name="builder">The host builder to configure</param>
    /// <returns>The configured and started host</returns>
    protected override IHost CreateHost(IHostBuilder builder)
    {
        _configureHost(builder);
        var host = builder.Build();
        host.Start();
        return host;
    }
}
