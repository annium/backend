using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;

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
            container.AddRuntimeTools(
                GetType().Assembly,
                true,
                typeof(Guid).Assembly.ShortName(),
                "Demo.Infrastructure.WebSockets.Client",
                "Demo.Infrastructure.WebSockets.Domain",
                "Annium.Infrastructure.WebSockets.Client",
                "Annium.Infrastructure.WebSockets.Domain"
            );
            container.AddTimeProvider();
            container.AddJsonSerializers()
                .Configure(opts => opts
                    .ConfigureForOperations()
                    .ConfigureForNodaTime()
                )
                .SetDefault();
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
            container.AddArguments();
            container.AddWebSocketClient();

            // commands
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .Singleton();
        }
    }
}