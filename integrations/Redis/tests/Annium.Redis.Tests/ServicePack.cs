using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.DependencyInjection.Packs;

namespace Annium.Redis.Tests;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRedis();
        container.Add<Database>().AsSelf().Singleton();
        container.Add(sp => sp.Resolve<Database>().Config).AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
#pragma warning disable VSTHRD002
        provider.Resolve<Database>().InitAsync().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }
}
