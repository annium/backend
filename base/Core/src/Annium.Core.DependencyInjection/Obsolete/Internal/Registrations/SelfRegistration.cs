using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal class SelfRegistration : IRegistration
    {
        public IEnumerable<ServiceDescriptor> ResolveServiceDescriptors(Type implementationType, ServiceLifetime lifetime)
        {
            yield return new ServiceDescriptor(implementationType, implementationType, lifetime);
        }
    }
}