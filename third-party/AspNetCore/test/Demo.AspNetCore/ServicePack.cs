using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.AspNetCore
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // register and setup services
            services.AddRuntimeTools(GetType().Assembly);
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            services.AddHttpRequestFactory();
            services.AddMediatorConfiguration(ConfigureMediator);
            services.AddMediator();
            services.AddLogging(route => route.UseInMemory());
        }

        private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager typeManager)
        {
            cfg.AddHttpStatusPipeHandler();
            cfg.AddCommandQueryHandlers(typeManager);
        }
    }
}