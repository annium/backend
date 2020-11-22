using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class TargetKeyedFactoryRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;
        private readonly Type _keyType;
        private readonly object _key;

        public TargetKeyedFactoryRegistration(Type serviceType, Type implementationType, Type keyType, object key)
        {
            _serviceType = serviceType;
            _implementationType = implementationType;
            _keyType = keyType;
            _key = key;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            yield return RegistrationHelper.CreateFuncFactoryDescriptor(_serviceType, _implementationType, lifetime);
            yield return RegistrationHelper.CreateFuncKeyFactoryDescriptor(_serviceType, _implementationType, _keyType, _key, lifetime);
        }
    }
}