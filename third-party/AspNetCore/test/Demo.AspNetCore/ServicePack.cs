using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.AspNetCore
{
    public class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            // register configurations
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // register and setup services
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            services.AddMediator();
            services.AddLogging(route => route.UseInMemory());
        }

        public override void Setup(System.IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}