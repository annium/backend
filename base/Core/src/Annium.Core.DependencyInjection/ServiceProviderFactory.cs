using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<IServiceProviderBuilder>
    {
        private readonly Action<ServiceProviderBuilder> configure;

        public ServiceProviderFactory()
        {
            configure = builder => { };
        }

        public ServiceProviderFactory(Action<IServiceProviderBuilder> configure)
        {
            this.configure = configure;
        }

        public IServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            var container = new ServiceProviderBuilder(services);
            configure(container);

            return container;
        }

        public IServiceProvider CreateServiceProvider(IServiceProviderBuilder container)
        {
            return ((ServiceProviderBuilder) container).Build();
        }
    }
}