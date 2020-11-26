using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime.Types;

namespace Annium.AspNetCore.TestServer
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            // register and setup services
            container.AddRuntimeTools(GetType().Assembly, true);
            container.AddTimeProvider();
            container.AddJsonSerializers(opts => opts
                .ConfigureForOperations()
                .ConfigureForNodaTime()
            );
            container.AddHttpRequestFactory();
            container.AddMediatorConfiguration(ConfigureMediator);
            container.AddMediator();
            container.AddLogging(route => route.UseInMemory());
        }

        private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager typeManager)
        {
            cfg.AddHttpStatusPipeHandler();
            cfg.AddCommandQueryHandlers(typeManager);
        }
    }
}