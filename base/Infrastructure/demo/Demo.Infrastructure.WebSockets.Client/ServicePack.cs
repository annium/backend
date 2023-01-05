using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Json;

namespace Demo.Infrastructure.WebSockets.Client;

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
        container.AddArguments();
        container.AddWebSocketClient();

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