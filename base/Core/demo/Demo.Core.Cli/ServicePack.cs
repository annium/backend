using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.Cli
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            // register configurations
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddAssemblyLoader();
        }

        public override void Setup(IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}