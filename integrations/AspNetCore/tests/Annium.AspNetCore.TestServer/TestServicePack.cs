using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Mesh.Transport.WebSockets;

namespace Annium.AspNetCore.TestServer;

/// <summary>
/// Service pack for test-specific server configuration with relative time and mesh transport
/// </summary>
public class TestServicePack : ServicePackBase
{
    /// <summary>
    /// Initializes a new instance of the TestServicePack class
    /// </summary>
    public TestServicePack()
    {
        Add<BaseServicePack>();
    }

    /// <summary>
    /// Registers test-specific services including relative time and WebSocket transport
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous registration.</returns>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        container.AddTime().WithRelativeTime().SetDefault();
        container.AddMeshWebSocketsClientTransport(_ => new ClientTransportConfiguration());
        return Task.CompletedTask;
    }
}
