using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal class TargetRegistration : IRegistration
    {
        private readonly Type _serviceType;

        public TargetRegistration(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public IEnumerable<ServiceDescriptor> ResolveServiceDescriptors(Type implementationType, ServiceLifetime lifetime)
        {
            if (implementationType == _serviceType)
                yield return new ServiceDescriptor(_serviceType, implementationType, lifetime);
            else
                yield return RegistrationHelper.CreateTypeFactoryDescriptor(_serviceType, implementationType, lifetime);
        }
    }
}