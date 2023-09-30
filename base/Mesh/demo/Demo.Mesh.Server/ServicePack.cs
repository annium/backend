using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Server;
using Demo.Infrastructure.WebSockets.Server.Handlers;

namespace Demo.Infrastructure.WebSockets.Server;

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
        container.AddSerializers()
            .WithJson(opts => opts
                .ConfigureForOperations()
                .ConfigureForNodaTime()
            );
        container.AddLogging();
        container.AddMapper();
        container.AddMediator();
        container.AddMediatorConfiguration(ConfigureMediator);
        container.AddWebSocketServer<ConnectionState>((_, _) => { });
        container.Add(new WebHostConfiguration()).AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }

    private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
    {
        cfg.AddWebSocketServerHandlers(tm);
    }
}