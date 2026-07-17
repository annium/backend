using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Data.Operations.Serialization.Json;
using Annium.Logging.Shared;
using Annium.Logging.Xunit;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Annium.AspNetCore.Mesh.TestServer;

/// <summary>
/// Service pack for the Mesh WebSockets middleware test server. Registers everything the middleware
/// needs except <c>IServerConnectionFactory&lt;WebSocket&gt;</c> and <c>ICoordinator</c> — those are
/// intentionally left unregistered here so each test host can supply its own scenario-specific double
/// (recording or throwing) via <see cref="Microsoft.Extensions.Hosting.IHostBuilder.ConfigureServices(System.Action{Microsoft.Extensions.Hosting.HostBuilderContext,Microsoft.Extensions.DependencyInjection.IServiceCollection})" />,
/// mirroring the pattern used by <c>Annium.AspNetCore.IntegrationTesting.Tests.KeyedTestHost</c>.
/// </summary>
public class ServicePack : ServicePackBase
{
    /// <summary>
    /// Registers logging, serialization and the Mesh WebSockets middleware itself.
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddSerializers().WithJson(opts => opts.ConfigureForOperations());
        container.AddLogging();
        container.AddMeshWebSocketsMiddleware(cfg => cfg.PathMatch = "/mesh");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Routes Annium logging to the xunit test output.
    /// </summary>
    /// <param name="provider">The service provider containing registered services</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous setup.</returns>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseTestOutput());
        return Task.CompletedTask;
    }
}
