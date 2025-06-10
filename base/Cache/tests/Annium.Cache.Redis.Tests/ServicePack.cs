using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Packs;

namespace Annium.Cache.Redis.Tests;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRedisCache(ServiceLifetime.Singleton);
    }
}
