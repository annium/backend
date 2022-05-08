using System;
using Annium.Core.DependencyInjection;

namespace Demo.Runtime.Loader
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddAssemblyLoader();
        }
    }
}