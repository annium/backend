using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;

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
            container.AddJsonSerializers((sp, opts) => opts
                .ConfigureDefault(sp.Resolve<ITypeManager>())
            );
        }

        public override void Setup(IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}