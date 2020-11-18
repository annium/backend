using System;
using Annium.Core.DependencyInjection.Obsolete.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.New
{
    [Obsolete]
    public class ServiceProviderFactory : IServiceProviderFactory<Obsolete.IServiceProviderBuilder>
    {
        private readonly Action<ServiceProviderBuilder> _configure;

        public ServiceProviderFactory()
        {
            _configure = _ => { };
        }

        public ServiceProviderFactory(Action<Obsolete.IServiceProviderBuilder> configure)
        {
            _configure = configure;
        }

        public Obsolete.IServiceProviderBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ServiceProviderBuilder(services);
            _configure(builder);

            return builder;
        }

        public IServiceProvider CreateServiceProvider(Obsolete.IServiceProviderBuilder container)
        {
            return ((ServiceProviderBuilder) container).Build();
        }
    }
}