using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private bool isAlreadyBuilt;

        private readonly IServiceCollection services;

        private readonly IList<ServicePackBase> packs = new List<ServicePackBase>();

        public ServiceProviderBuilder()
        {
            services = new ServiceCollection();
        }

        public ServiceProviderBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public IServiceProviderBuilder UseServicePack<TServicePack>()
            where TServicePack : ServicePackBase, new()
        {
            if (packs.All(e => e.GetType() != typeof(TServicePack)))
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
            var provider = services.BuildServiceProvider();

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