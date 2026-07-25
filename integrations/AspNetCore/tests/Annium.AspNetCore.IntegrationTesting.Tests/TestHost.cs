using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Test host for the <c>Annium.AspNetCore.IntegrationTesting.Tests</c> suite. Configures the ASP.NET Core
/// test server with <see cref="TestServicePack" />; no host-specific start/stop work is required, so the
/// base class's lifecycle hooks are left as-is.
/// </summary>
internal class TestHost : TestHostBase<Program>
{
    public TestHost(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        //
    }

    /// <summary>
    /// Configures the host builder by applying <see cref="TestServicePack" />, which registers the
    /// service dependencies needed by the integration testing test suite.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
    }
}
