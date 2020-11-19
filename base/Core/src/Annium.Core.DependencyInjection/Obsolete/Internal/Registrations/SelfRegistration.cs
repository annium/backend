using System;
using System.Collections.Generic;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal class SelfRegistration : IRegistration
    {
        public IEnumerable<MicrosoftServiceDescriptor> ResolveServiceDescriptors(Type implementationType, MicrosoftServiceLifetime lifetime)
        {
            yield return new MicrosoftServiceDescriptor(implementationType, implementationType, lifetime);
        }
    }
}