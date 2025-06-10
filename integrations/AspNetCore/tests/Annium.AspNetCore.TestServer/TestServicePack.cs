using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Packs;
using Annium.Core.Runtime;
using Annium.Mesh.Transport.WebSockets;

namespace Annium.AspNetCore.TestServer;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Add<BaseServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRelativeTime().SetDefault();
        container.AddMeshWebSocketsClientTransport(_ => new ClientTransportConfiguration());
    }
}
