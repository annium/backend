using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test host for the <c>Annium.AspNetCore.Extensions.Tests</c> suite. Configures the ASP.NET Core
/// test server with <see cref="TestServicePack" />; no host-specific start/stop work is required, so the
/// base class's lifecycle hooks are left as-is.
/// </summary>
internal class TestHost : TestHostBase<Program>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestHost"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the host logs through.</param>
    public TestHost(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        //
    }

    /// <summary>
    /// Configures the host builder by applying <see cref="TestServicePack" />, which registers the
    /// service dependencies needed by the extensions test suite.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
    }
}
