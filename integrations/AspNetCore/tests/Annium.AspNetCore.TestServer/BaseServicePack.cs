using System;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Handlers;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Infrastructure.WebSockets.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

internal class BaseServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register and setup services
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddSerializers()
            .WithJson(opts => opts
                .ConfigureForOperations()
                .ConfigureForNodaTime()
            );
        container.AddLogging();
        container.AddHttpRequestFactory().SetDefault();
        container.AddMediatorConfiguration(ConfigureMediator);
        container.AddMediator();
        container.AddWebSocketServer<ConnectionState>(
            (_, cfg) => cfg
                .ListenOn("/ws")
                .UseFormat(SerializationFormat.Text)
                .WithActiveKeepAlive(600)
        );
        container.Add<SharedDataContainer>().AsSelf().Singleton();

        // server
        container.Collection.AddControllers();
        container.Collection.AddCors();
        container.Collection.AddMvc()
            .AddDefaultJsonOptions();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseTestConsole());
    }

    private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
    {
        cfg.AddWebSocketServerHandlers(tm);
        cfg.AddHttpStatusPipeHandler();
        cfg.AddCommandQueryHandlers(tm);
    }
}