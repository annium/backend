using System;
using Annium.Core.DependencyInjection;
using Annium.Mesh.Transport.WebSockets;

namespace Demo.Mesh.Client;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Domain.ServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddMapper();
        container.AddArguments();
        container.AddMeshClient();
        container.AddMeshWebSocketsClientTransport(_ => new ClientTransportConfiguration
        {
            Uri = new Uri("ws://localhost:2727")
        });
        container.AddMeshJsonSerialization();

        // commands
        container.AddAll(GetType().Assembly)
            .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
            .AsSelf()
            .Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}