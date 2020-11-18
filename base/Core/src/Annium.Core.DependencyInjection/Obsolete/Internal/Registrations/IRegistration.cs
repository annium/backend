using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Obsolete.Internal.Registrations
{
    internal interface IRegistration
    {
        IEnumerable<ServiceDescriptor> ResolveServiceDescriptors(Type implementationType, ServiceLifetime lifetime);
    }
}