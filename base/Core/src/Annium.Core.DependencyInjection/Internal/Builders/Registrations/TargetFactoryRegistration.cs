using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders.Registrations
{
    internal class TargetFactoryRegistration : IRegistration
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;

        public TargetFactoryRegistration(Type serviceType, Type implementationType)
        {
            _serviceType = serviceType;
            _implementationType = implementationType;
        }

        public IEnumerable<IServiceDescriptor> ResolveServiceDescriptors(ServiceLifetime lifetime)
        {
            yield return RegistrationHelper.CreateFuncFactoryDescriptor(_serviceType, _implementationType, lifetime);
        }
    }
}