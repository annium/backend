using System;
using Annium.Core.DependencyInjection;

namespace Demo.Infrastructure.WebSockets.Client
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Domain.ServicePack>();
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddTimeProvider();
            container.AddJsonSerializers(opts => opts
                .ConfigureForOperations()
                .ConfigureForNodaTime()
            );
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
            container.AddArguments();

            // commands
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .Singleton();
        }
    }
}