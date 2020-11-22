using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class InstanceKeyedFactoryRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly object _instance;
        private readonly Type _keyType;
        private readonly object _key;

        public InstanceKeyedFactoryRegistration(Type serviceType, object instance, Type keyType, object key)
        {
            _serviceType = serviceType;
            _instance = instance;
            _keyType = keyType;
            _key = key;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            // TODO:
            yield break;
        }
    }
}