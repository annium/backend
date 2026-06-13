using System.Threading.Tasks;
using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.Extensions.Tests;

/// <summary>
/// Test host for the <c>Annium.AspNetCore.Extensions.Tests</c> suite. Configures the ASP.NET Core
/// test server with <see cref="TestServicePack" /> and provides no-op start/stop lifecycle hooks.
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
    /// service dependencies needed by the extensions test suite.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
    }

    /// <summary>
    /// Called after the underlying host has started. No additional start-up work is required for
    /// this test host.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask" />.</returns>
    protected override ValueTask HandleStartAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Called before the underlying host is stopped. No additional teardown work is required for
    /// this test host.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask" />.</returns>
    protected override ValueTask HandleStopAsync()
    {
        return ValueTask.CompletedTask;
    }
}
