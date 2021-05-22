using System;
using Annium.AspNetCore.TestServer.Handlers;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;
using Annium.Infrastructure.WebSockets.Domain;

namespace Annium.AspNetCore.TestServer
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            // register and setup services
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddTimeProvider();
            container.AddJsonSerializers()
                .Configure(opts => opts
                    .ConfigureForOperations()
                    .ConfigureForNodaTime()
                );
            container.AddLogging(route => route.UseConsole());
            container.AddHttpRequestFactory().SetDefault();
            container.AddMediatorConfiguration(ConfigureMediator);
            container.AddMediator();
            container.AddLogging(route => route.UseInMemory());
            container.AddWebSocketServer(
                (_, cfg) => cfg
                    .UseFormat(SerializationFormat.Text)
                    .WithActiveKeepAlive(600),
                connectionId => new ConnectionState(connectionId)
            );
        }

        private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager typeManager)
        {
            cfg.AddHttpStatusPipeHandler();
            cfg.AddCommandQueryHandlers(typeManager);
        }
    }
}