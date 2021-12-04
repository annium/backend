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

        container.AddRuntimeTools(Assembly.GetCallingAssembly(), false);
        container.AddTime().WithRealTime().SetDefault();

        container.AddLogging(route => route.UseInMemory());

        container.AddLocalization(opts => opts.UseInMemoryStorage());

        container.AddComposition();
        container.AddValidation();

        container.AddMediatorConfiguration(configure);
        container.AddMediator();

        return container.BuildServiceProvider().Resolve<IMediator>();
    }
}