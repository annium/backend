using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class TargetRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;

        public TargetRegistration(Type serviceType, Type implementationType)
        {
            _serviceType = serviceType;
            _implementationType = implementationType;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            if (_implementationType == _serviceType)
                yield return ServiceDescriptor.Type(_serviceType, _implementationType, lifetime);
            else
                yield return RegistrationHelper.CreateTypeFactoryDescriptor(_serviceType, _implementationType, lifetime);
        }
    }
}