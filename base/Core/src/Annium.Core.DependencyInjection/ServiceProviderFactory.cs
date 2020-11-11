using System;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceProviderFactory : IServiceProviderFactory<IServiceProviderBuilder>
    {
        private readonly Action<ServiceProviderBuilder> _configure;

        public ServiceProviderFactory()
        {
            _configure = builder => { };
        }

        public ServiceProviderFactory(Action<IServiceProviderBuilder> configure)
        {
            _configure = configure;
        }

        public IServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            var container = new ServiceProviderBuilder(services);
            _configure(container);

            return container;
        }

        public IServiceProvider CreateServiceProvider(IServiceProviderBuilder container)
        {
            return ((ServiceProviderBuilder) container).Build();
        }
    }
}