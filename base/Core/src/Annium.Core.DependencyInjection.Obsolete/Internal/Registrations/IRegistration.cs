using System;
using System.Collections.Generic;
using MicrosoftServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
using MicrosoftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal interface IRegistration
    {
        IEnumerable<MicrosoftServiceDescriptor> ResolveServiceDescriptors(Type implementationType, MicrosoftServiceLifetime lifetime);
    }
}