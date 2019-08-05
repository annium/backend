using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public abstract class ServicePackBase
    {
        private readonly IList<ServicePackBase> packs = new List<ServicePackBase>();

        public void Add<TServicePack>()
        where TServicePack : ServicePackBase, new()
        {
            packs.Add(new TServicePack());
        }

        public virtual void Configure(IServiceCollection services) { }

        public virtual void Register(IServiceCollection services, IServiceProvider provider) { }

        public virtual void Setup(IServiceProvider provider) { }

        internal void InternalConfigure(IServiceCollection services)
        {
            foreach (var pack in this.packs)
                pack.InternalConfigure(services);

            Configure(services);
        }

        internal void InternalRegister(IServiceCollection services, IServiceProvider provider)
        {
            foreach (var pack in this.packs)
                pack.InternalRegister(services, provider);

            Register(services, provider);
        }

        internal void InternalSetup(IServiceProvider provider)
        {
            foreach (var pack in this.packs)
                pack.InternalSetup(provider);

            Setup(provider);
        }
    }
}