using System;
using System.Collections.Generic;
using static Annium.Core.DependencyInjection.Internal.Builders.Registrations.Helper;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class TypeKeyedRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;
        private readonly Type _keyType;
        private readonly object _key;

        public TypeKeyedRegistration(Type serviceType, Type implementationType, Type keyType, object key)
        {
            _serviceType = serviceType;
            _implementationType = implementationType;
            _keyType = keyType;
            _key = key;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            yield return Factory(
                KeyValueType(_keyType, _serviceType),
                sp => KeyValue(_keyType, _serviceType, _key, Resolve(sp, _implementationType)),
                lifetime
            );
        }
    }
}