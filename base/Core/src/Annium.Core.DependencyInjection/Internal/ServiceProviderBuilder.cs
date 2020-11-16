using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal
{
    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private bool _isAlreadyBuilt;

        private readonly IServiceCollection _services;

        private readonly IList<ServicePackBase> _packs = new List<ServicePackBase>();

        public ServiceProviderBuilder()
        {
            _services = new ServiceCollection();
        }

        public ServiceProviderBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public IServiceProviderBuilder UseServicePack<TServicePack>()
            where TServicePack : ServicePackBase, new()
        {
            if (_packs.All(e => e.GetType() != typeof(TServicePack)))
                _packs.Add(new TServicePack());

            return this;
        }

        public ServiceProvider Build()
        {
            if (_isAlreadyBuilt)
                throw new InvalidOperationException("Entrypoint is already built");
            _isAlreadyBuilt = true;

            // configure all packs
            var configurationServices = new ServiceCollection();
            foreach (var pack in _packs)
                pack.InternalConfigure(configurationServices);

            // copy all configuration services to services
            foreach (var descriptor in configurationServices)
                _services.Add(descriptor);

            // create provider from configurationServices
            var provider = _services.BuildServiceProvider();

            // register all services from packs
            foreach (var pack in _packs)
                pack.InternalRegister(_services, provider);

            // create provider from actual services
            provider = _services.BuildServiceProvider();

            // setup all services from packs
            foreach (var pack in _packs)
                pack.InternalSetup(provider);

            return provider;
        }
    }
}