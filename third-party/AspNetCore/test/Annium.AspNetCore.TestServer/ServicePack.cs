using System;
using Annium.AspNetCore.TestServer.Components;
using Annium.AspNetCore.TestServer.Handlers;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Infrastructure.WebSockets.Server;

namespace Annium.AspNetCore.TestServer;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register and setup services
        container.AddRuntimeTools(GetType().Assembly, true);
        container.AddTime().WithRealTime().SetDefault();
        container.AddJsonSerializers()
            .Configure(opts => opts
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