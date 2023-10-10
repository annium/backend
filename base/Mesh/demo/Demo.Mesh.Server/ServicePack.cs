using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.WebSockets;
using Demo.Mesh.Server.Handlers;

namespace Demo.Mesh.Server;

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
        container.AddMeshJsonSerialization();
        container.AddLogging();
        container.AddMapper();
        container.AddMediator();
        container.AddMediatorConfiguration(ConfigureMediator);
        container.AddMeshServer<ConnectionState>();
        container.AddMeshWebSocketsServerTransport(_ => new ServerTransportConfiguration());
        container.AddSocketServerMeshHandler();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }

    private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
    {
        cfg.AddMeshServerHandlers(tm);
    }
}