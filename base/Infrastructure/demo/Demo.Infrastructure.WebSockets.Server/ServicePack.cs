using System;
using Annium.Configuration.Abstractions;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain;
using Annium.Infrastructure.WebSockets.Server;
using Demo.Infrastructure.WebSockets.Server.Handlers;

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
            container.AddJsonSerializers()
                .Configure(opts => opts
                    .ConfigureForOperations()
                    .ConfigureForNodaTime()
                )
                .SetDefault();
            container.AddLogging(route => route.UseConsole());
            container.AddMapper();
            container.AddMediator();
            container.AddMediatorConfiguration(ConfigureMediator);
            container.AddWebSocketServer(
                (sp, cfg) => cfg
                    .UseFormat(sp.Resolve<Configuration>().UseText ? SerializationFormat.Text : SerializationFormat.Binary)
                    .WithActiveKeepAlive(1),
                connectionId => new ConnectionState(connectionId)
            );
        }

        private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
        {
            cfg.AddWebSocketServerHandlers(tm);
        }
    }
}