using System;
using System.Collections.Generic;
using System.Linq;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal class InterfacesFactoriesRegistration : IRegistration
    {
        public IEnumerable<MicrosoftServiceDescriptor> ResolveServiceDescriptors(Type implementationType, MicrosoftServiceLifetime lifetime)
        {
            return implementationType
                .GetInterfaces()
                .Select(x => RegistrationHelper.CreateFuncFactoryDescriptor(x, implementationType, lifetime));
        }
    }
}