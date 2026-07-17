using System;
using Annium.AspNetCore.TestServer;
using Annium.Infrastructure.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Test host that additionally registers a scoped <see cref="IScopedMarker" /> service directly on
/// the ASP.NET Core container (via <see cref="IHostBuilder.ConfigureServices" />), independent of the
/// shared <see cref="Annium.AspNetCore.TestServer" /> registrations, so that
/// <see cref="TestHostBase{TEntryPoint}.CreateAsyncScope" /> has a scoped service to resolve against.
/// </summary>
internal class ScopedTestHost : TestHostBase<Program>
{
    /// <summary>
    /// Initializes a new instance of the ScopedTestHost class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public ScopedTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Configures the host builder by applying <see cref="TestServicePack" /> and then additionally
    /// registering a scoped <see cref="IScopedMarker" /> service directly on the ASP.NET Core
    /// container. This is purely additive: it does not touch any registration used by other test hosts.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<TestServicePack>();
        builder.ConfigureServices(services => services.AddScoped<IScopedMarker, ScopedMarker>());
    }
}

/// <summary>
/// Marker service used to pin the scoped-resolution behavior of <see cref="TestHostBase{TEntryPoint}.CreateAsyncScope" />.
/// </summary>
internal interface IScopedMarker
{
    /// <summary>
    /// Gets the identity of this instance, unique per DI scope.
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Default implementation of <see cref="IScopedMarker" />, assigning itself a fresh identity on construction
/// so that instances resolved from different scopes can be told apart.
/// </summary>
internal sealed class ScopedMarker : IScopedMarker
{
    /// <summary>
    /// Gets the identity of this instance, unique per DI scope.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
}
