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
#pragma warning disable VSTHRD002
        Database.AcquireAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }
}
