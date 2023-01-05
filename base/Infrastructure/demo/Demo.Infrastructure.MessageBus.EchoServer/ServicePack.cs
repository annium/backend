using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

namespace Demo.Infrastructure.MessageBus.EchoServer;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddSerializers().WithJson();
        container.AddMapper();
        container.AddConfiguration<EndpointsConfiguration>(x => x.AddYamlFile("cfg_local.yml"));
        container.AddNetMQMessageBus((sp, opts) => opts
            .WithSerializer(sp.Resolve<ISerializer<string>>())
            .WithEndpoints(sp.Resolve<EndpointsConfiguration>())
        );
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseConsole());
    }
}