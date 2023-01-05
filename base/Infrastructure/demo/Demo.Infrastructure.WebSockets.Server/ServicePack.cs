using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Infrastructure.WebSockets.Server;
using Annium.Serialization.Json;
using Demo.Infrastructure.WebSockets.Server.Handlers;

namespace Demo.Infrastructure.WebSockets.Server;

internal class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<Domain.ServicePack>();
    }

    public override void Configure(IServiceContainer container)
    {
        container.AddConfiguration<Configuration>(cfg => cfg.AddCommandLineArgs());
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
        container.AddWebSocketServer<ConnectionState>(
            (sp, cfg) => cfg
                .UseFormat(sp.Resolve<Configuration>().UseText ? SerializationFormat.Text : SerializationFormat.Binary)
                .WithActiveKeepAlive(600)
        );
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