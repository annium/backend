using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal class SelfFactoryRegistration : IRegistration
    {
        public IEnumerable<ServiceDescriptor> ResolveServiceDescriptors(Type implementationType, ServiceLifetime lifetime)
        {
            yield return RegistrationHelper.CreateFuncFactoryDescriptor(implementationType, implementationType, lifetime);
        }
    }
}