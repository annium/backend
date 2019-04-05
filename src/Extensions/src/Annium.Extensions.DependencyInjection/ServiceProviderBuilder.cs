using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.DependencyInjection
{
    public class ServiceProviderBuilder
    {
        private bool isAlreadyBuilt = false;

        private IServiceCollection services { get; }

        private IList<ServicePackBase> packs { get; } = new List<ServicePackBase>();

        public ServiceProviderBuilder(IServiceCollection services = null)
        {
            this.services = services ?? new ServiceCollection();
        }

        public ServiceProviderBuilder UseServicePack<TServicePack>()
        where TServicePack : ServicePackBase, new()
        {
            if (!packs.Any(e => e.GetType() == typeof(TServicePack)))
                packs.Add(new TServicePack());

            return this;
        }

        public ServiceProvider Build()
        {
            if (isAlreadyBuilt)
                throw new InvalidOperationException($"Entrypoint is already built");
            isAlreadyBuilt = true;

            // configure all packs
            var configurationServices = new ServiceCollection();
            foreach (var pack in packs)
                pack.InternalConfigure(configurationServices);

            // copy all configuration services to services
            foreach (var descriptor in configurationServices)
                services.Add(descriptor);

            // create provider from configurationServices
            var provider = configurationServices.BuildServiceProvider();

            // register all services from packs
            foreach (var pack in packs)
                pack.InternalRegister(services, provider);

            // create provider from actual services
            provider = services.BuildServiceProvider();

            // setup all services from packs
            foreach (var pack in packs)
                pack.InternalSetup(provider);

            return provider;
        }
    }
}