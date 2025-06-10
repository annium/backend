using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Packs;
using Annium.Core.Runtime;

namespace Annium.AspNetCore.TestServer;

public class ServicePack : ServicePackBase
{
    public ServicePack()
    {
        Add<BaseServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();
    }
}
