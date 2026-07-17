using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Test host that additionally registers a <see cref="RequestGate" />-backed <c>/slow</c> endpoint via
/// <see cref="SlowRequestStartupFilter" />, directly on the ASP.NET Core container (via
/// <see cref="IHostBuilder.ConfigureServices" />), independent of the shared
/// <see cref="Annium.AspNetCore.TestServer" /> registrations, so tests can hold a request in flight while
/// exercising <see cref="TestHostBase{TEntryPoint}.DisposeAsync" /> concurrently.
/// </summary>
internal class SlowRequestTestHost : TestHostBase<Program>
{
    /// <summary>
    /// The gate controlling when the <c>/slow</c> endpoint completes its in-flight request.
    /// </summary>
    public RequestGate Gate { get; } = new();

    /// <summary>
    /// Initializes a new instance of the SlowRequestTestHost class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public SlowRequestTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Configures the host builder by applying <see cref="TestServicePack" /> and then additionally
    /// registering the <see cref="Gate" /> and the <see cref="SlowRequestStartupFilter" /> that maps the
    /// <c>/slow</c> endpoint. This is purely additive: it does not touch any registration used by other
    /// test hosts.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Gate);
            services.AddSingleton<IStartupFilter, SlowRequestStartupFilter>();
        });
    }
}
