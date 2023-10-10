using System;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Handlers;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.WebSockets;
using Annium.Net.WebSockets;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

internal class BaseServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register and setup services
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers()
            .WithJson(opts => opts
                .ConfigureForOperations()
                .ConfigureForNodaTime()
            );
        container.AddLogging();
        container.AddHttpRequestFactory(true);
        container.AddMediatorConfiguration(ConfigureMediator);
        container.AddMediator();
        container.AddMeshServer<ConnectionState>();
        container.AddMeshWebSocketsServerTransport(_ => new ServerTransportConfiguration
        {
            ConnectionMonitor = ConnectionMonitor.None
        });
        container.AddMeshWebSocketsMiddleware(cfg => cfg.PathMatch = "/ws");
        container.Add<SharedDataContainer>().AsSelf().Singleton();

        // server
        container.Collection.AddControllers();
        container.Collection.AddCors();
        container.Collection.AddMvc()
            .AddDefaultJsonOptions();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseTestOutput());
    }

    private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
    {
        cfg.AddMeshServerHandlers(tm);
        cfg.AddHttpStatusPipeHandler();
        cfg.AddCommandQueryHandlers(tm);
    }
}