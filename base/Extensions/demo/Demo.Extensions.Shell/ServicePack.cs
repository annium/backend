using System;
using Annium.Core.DependencyInjection;

namespace Demo.Extensions.Shell
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceContainer container)
        {
            // register configurations
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();
            container.AddLogging(route => route.UseConsole());
            container.AddShell();
        }

        public override void Setup(IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}