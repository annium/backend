using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class TargetKeyedRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;
        private readonly Type _keyType;
        private readonly object _key;

        public TargetKeyedRegistration(Type serviceType, Type implementationType, Type keyType, object key)
        {
            _serviceType = serviceType;
            _implementationType = implementationType;
            _keyType = keyType;
            _key = key;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            if (_implementationType != _serviceType)
                yield return RegistrationHelper.CreateTypeFactoryDescriptor(_serviceType, _implementationType, lifetime);
            yield return RegistrationHelper.CreateTypeKeyFactoryDescriptor(_serviceType, _implementationType, _keyType, _key, lifetime);
        }
    }
}