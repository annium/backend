using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;

namespace Annium.Architecture.Mediator.Tests;

public class TestBase
{
    protected IMediator GetMediator(Action<MediatorConfiguration> configure)
    {
        var container = new ServiceContainer();

        container.AddRuntime(Assembly.GetCallingAssembly());
        container.AddTime().WithRealTime().SetDefault();

        container.AddLogging();

        container.AddLocalization(opts => opts.UseInMemoryStorage());

        container.AddComposition();
        container.AddValidation();

        container.AddMediatorConfiguration(configure);
        container.AddMediator();

        var sp = container.BuildServiceProvider();

        sp.UseLogging(route => route.UseInMemory());

        return sp.Resolve<IMediator>();
    }
}