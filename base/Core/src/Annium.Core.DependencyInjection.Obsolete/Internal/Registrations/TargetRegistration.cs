using System;
using System.Collections.Generic;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal class TargetRegistration : IRegistration
    {
        private readonly Type _serviceType;

        public TargetRegistration(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public IEnumerable<MicrosoftServiceDescriptor> ResolveServiceDescriptors(Type implementationType, MicrosoftServiceLifetime lifetime)
        {
            if (implementationType == _serviceType)
                yield return new MicrosoftServiceDescriptor(_serviceType, implementationType, lifetime);
            else
                yield return RegistrationHelper.CreateTypeFactoryDescriptor(_serviceType, implementationType, lifetime);
        }
    }
}