using System;
using Annium.Core.DependencyInjection;

namespace Demo.Extensions.Jobs;

internal class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();
        container.AddScheduler();

        container.Add<Job>().AsSelf().Singleton();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(x => x.UseConsole());
    }
}