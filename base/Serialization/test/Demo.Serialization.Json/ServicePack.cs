using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Serialization.Json
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