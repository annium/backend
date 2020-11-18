using System;
using Annium.Core.DependencyInjection;
using Annium.Core.DependencyInjection.Obsolete;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.Extensions.Shell
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            // register configurations
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            services.AddLogging(route => route.UseConsole());
            services.AddShell();
        }

        public override void Setup(IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}