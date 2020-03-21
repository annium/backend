using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.AspNetCore
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // register and setup services
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            services.AddMediatorConfiguration(ConfigureMediator);
            services.AddMediator();
            services.AddLogging(route => route.UseInMemory());
        }

        private void ConfigureMediator(MediatorConfiguration cfg)
        {
            cfg.AddHttpStatusPipeHandler();
            cfg.AddModelStatePipeHandler();

            cfg.AddCommandQueryHandlers();
        }
    }
}