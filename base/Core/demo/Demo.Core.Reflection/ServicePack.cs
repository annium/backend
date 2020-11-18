using System;
using Annium.Core.DependencyInjection;
using Annium.Core.DependencyInjection.Obsolete;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.Reflection
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

        public override void Setup(IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}