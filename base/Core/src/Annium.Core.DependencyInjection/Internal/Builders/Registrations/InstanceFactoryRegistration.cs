using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class InstanceFactoryRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly object _instance;

        public InstanceFactoryRegistration(Type serviceType, object instance)
        {
            _serviceType = serviceType;
            _instance = instance;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            // TODO:
            yield break;
            // yield return RegistrationHelper.CreateInstanceFactoryDescriptor(_serviceType, _implementationType, lifetime);
        }
    }
}