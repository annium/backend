using System;
using Annium.Core.DependencyInjection;

namespace Annium.Redis.Tests;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRedis();
        container.Add(Database.Config).AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        Database.AcquireAsync().GetAwaiter().GetResult();
    }
}