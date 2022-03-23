using System;
using Annium.Core.DependencyInjection;

namespace Demo.Net.WebSockets.Server;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        // register configurations
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register and setup services
        container.Add(new WebHostConfiguration()).AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        // setup post-configured services
    }
}