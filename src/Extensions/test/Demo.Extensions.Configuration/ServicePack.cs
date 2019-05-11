using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Configuration
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            // register configurations
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            // register and setup services
        }

        public override void Setup(System.IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}