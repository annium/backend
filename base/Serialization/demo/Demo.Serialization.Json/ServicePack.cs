using System;
using Annium.Core.DependencyInjection;

namespace Demo.Serialization.Json
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceContainer container)
        {
            // register configurations
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddJsonSerializers();
        }

        public override void Setup(IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}