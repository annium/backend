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
/// Test host that supplies a <see cref="ThrowingConnectionFactory" /> (failing before a connection is ever
/// created) alongside a <see cref="RecordingCoordinator" />, so tests can pin the middleware's catch-all
/// failure branch and confirm the coordinator is never invoked when the connection factory fails first.
/// </summary>
internal class ThrowingFactoryTestHost : TestHostBase<Program>
{
    /// <summary>
    /// The connection factory double supplied to this host's DI container, used to await the failure path
    /// having deterministically run.
    /// </summary>
    public ThrowingConnectionFactory ConnectionFactory { get; } = new();

    /// <summary>
    /// The coordinator double supplied to this host's DI container, used to assert it is never invoked.
    /// </summary>
    public RecordingCoordinator Coordinator { get; } = new();

    /// <summary>
    /// Fires once the request pipeline's wrapping middleware confirms <c>WebSocketsMiddleware.InvokeAsync</c>
    /// has fully returned for a given request, giving tests a deterministic point at which to inspect
    /// <see cref="Coordinator" /> without racing a bounded timer against it.
    /// </summary>
    public RequestCompletionSignal RequestCompleted { get; } = new();

    /// <summary>
    /// Records any exception that escapes <c>WebSocketsMiddleware.InvokeAsync</c> for a request, instead of
    /// letting the test host's wrapping middleware swallow it silently.
    /// </summary>
    public EscapedExceptionSink EscapedException { get; } = new();

    /// <summary>
    /// Initializes a new instance of the ThrowingFactoryTestHost class
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging</param>
    public ThrowingFactoryTestHost(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Configures the host builder by applying <see cref="ServicePack" /> and then registering a throwing
    /// connection factory and a recording coordinator.
    /// </summary>
    /// <param name="builder">The <see cref="IHostBuilder" /> to configure before the host is built.</param>
    protected override void ConfigureHost(IHostBuilder builder)
    {
        builder.UseServicePack<ServicePack>();
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IServerConnectionFactory<WebSocket>>(ConnectionFactory);
            services.AddSingleton<ICoordinator>(Coordinator);
            services.AddSingleton(RequestCompleted);
            services.AddSingleton(EscapedException);
        });
    }
}
