using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Server;

namespace Demo.Infrastructure.WebSockets.Server
{
    internal class ServicePack : ServicePackBase
    {
        public ServicePack()
        {
            Add<Domain.ServicePack>();
        }

        public override void Configure(IServiceContainer container)
        {
            container.AddConfiguration<Configuration>(cfg => cfg.AddCommandLineArgs());
        }

        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddTimeProvider();
            container.AddJsonSerializers(opts => opts
                .ConfigureForOperations()
                .ConfigureForNodaTime()
            );
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
            container.AddMediator();
            container.AddMediatorConfiguration(ConfigureMediator);
            container.AddWebSocketServer();
        }

        private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
        {
            cfg.AddWebSocketServerHandlers(tm);
        }
    }
}