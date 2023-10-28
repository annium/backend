using System;
using Annium.Core.DependencyInjection;

namespace Annium.Cache.Redis.Tests;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRedisCache(ServiceLifetime.Singleton);
    }
}
