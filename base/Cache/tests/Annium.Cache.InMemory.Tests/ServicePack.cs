using System;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.InMemory.Tests;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddInMemoryCache(ServiceLifetime.Singleton);
    }
}
