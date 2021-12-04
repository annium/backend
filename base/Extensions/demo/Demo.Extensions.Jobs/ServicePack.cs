using System;
using Annium.Core.DependencyInjection;

namespace Demo.Extensions.Jobs;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddScheduler();
    }
}