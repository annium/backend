using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class KeyedFactoryRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly Func<IServiceProvider, object> _factory;
        private readonly Type _keyType;
        private readonly object _key;

        public KeyedFactoryRegistration(Type serviceType, Func<IServiceProvider, object> factory, Type keyType, object key)
        {
            _serviceType = serviceType;
            _factory = factory;
            _keyType = keyType;
            _key = key;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            yield return ServiceDescriptor.Factory(_serviceType, _factory, lifetime);
            yield return RegistrationHelper.CreateTypeKeyFactoryDescriptor(_serviceType, _serviceType, _keyType, _key, lifetime);
        }
    }
}