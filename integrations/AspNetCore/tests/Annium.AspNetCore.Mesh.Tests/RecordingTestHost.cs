using System.Net.WebSockets;
using Annium.AspNetCore.IntegrationTesting;
using Annium.AspNetCore.Mesh.Tests.TestDoubles;
using Annium.AspNetCore.Mesh.TestServer;
using Annium.Infrastructure.Hosting;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Annium.AspNetCore.Mesh.Tests;

/// <summary>
/// Test host that supplies a <see cref="RecordingConnectionFactory" /> and <see cref="RecordingCoordinator" />
/// in place of the real Mesh server stack, so tests can pin the middleware's happy-path wiring (accept →
/// create connection → hand off to coordinator) and its ApplicationStopping → coordinator.Dispose hookup
/// without needing a full real Mesh server. Registered directly via <see cref="IHostBuilder.ConfigureServices(System.Action{HostBuilderContext,IServiceCollection})" />,
/// independent of <see cref="ServicePack" />, mirroring <c>Annium.AspNetCore.IntegrationTesting.Tests.KeyedTestHost</c>.
/// </summary>
internal class RecordingTestHost : TestHostBase<Program>
{
    /// <summary>
    /// The connection factory double supplied to this host's DI container.
    /// </summary>
    public RecordingConnectionFactory ConnectionFactory { get; } = new();

    /// <summary>
    /// The coordinator double supplied to this host's DI container.
    /// </summary>
    public RecordingCoordinator Coordinator { get; } = new();

    /// <summary>
    /// Initializes a new instance of the RecordingTestHost class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public RecordingTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Configures the host builder by applying <see cref="ServicePack" /> and then registering the recording
    /// doubles as the connection factory and coordinator consumed by the middleware.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<ServicePack>();
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IServerConnectionFactory<WebSocket>>(ConnectionFactory);
            services.AddSingleton<ICoordinator>(Coordinator);
        });
    }
}
