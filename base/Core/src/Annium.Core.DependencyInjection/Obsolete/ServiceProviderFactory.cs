using System;
using Annium.Core.DependencyInjection.Obsolete.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete
{
    [Obsolete]
    public class ServiceProviderFactory : IServiceProviderFactory<IServiceProviderBuilder>
    {
        private readonly Action<ServiceProviderBuilder> _configure;

        public ServiceProviderFactory()
        {
            _configure = _ => { };
        }

        public ServiceProviderFactory(Action<IServiceProviderBuilder> configure)
        {
            _configure = configure;
        }

        public IServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ServiceProviderBuilder(services);
            _configure(builder);

            return builder;
        }

        public IServiceProvider CreateServiceProvider(IServiceProviderBuilder container)
        {
            return ((ServiceProviderBuilder) container).Build();
        }
    }
}