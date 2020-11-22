using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using NodaTime;

namespace Annium.Architecture.Mediator.Tests
{
    public class TestBase
    {
        protected IMediator GetMediator(Action<MediatorConfiguration> configure)
        {
            var container = new ServiceContainer();

            container.AddRuntimeTools(Assembly.GetCallingAssembly(), false);
            container.Add<Func<Instant>>(SystemClock.Instance.GetCurrentInstant).Singleton();

            container.AddLogging(route => route.UseInMemory());

            container.AddLocalization(opts => opts.UseInMemoryStorage());

            container.AddComposition();
            container.AddValidation();

            container.AddMediatorConfiguration(configure);
            container.AddMediator();

            return container.BuildServiceProvider().Resolve<IMediator>();
        }
    }
}