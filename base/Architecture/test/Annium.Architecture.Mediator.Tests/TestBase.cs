using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Architecture.Mediator.Tests
{
    public class TestBase
    {
        protected IMediator GetMediator(Action<MediatorConfiguration> configure)
        {
            var services = new ServiceCollection();

            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            services.AddLogging(route => route.UseInMemory());

            services.AddLocalization(opts => opts.UseInMemoryStorage());

            services.AddComposition();
            services.AddValidation();

            services.AddMediatorConfiguration(configure);
            services.AddMediator();

            return services.BuildServiceProvider().GetRequiredService<IMediator>();
        }
    }
}