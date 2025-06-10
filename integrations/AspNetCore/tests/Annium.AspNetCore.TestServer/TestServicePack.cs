using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Packs;
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
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRelativeTime().SetDefault();
        container.AddMeshWebSocketsClientTransport(_ => new ClientTransportConfiguration());
    }
}
