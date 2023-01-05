using System;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Json;

namespace Demo.Serialization.Json;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        // register configurations
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers().WithJson();
    }

    public override void Setup(IServiceProvider provider)
    {
        // setup post-configured services
    }
}