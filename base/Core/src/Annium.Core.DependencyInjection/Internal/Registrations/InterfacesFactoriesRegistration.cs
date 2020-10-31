using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal class InterfacesFactoriesRegistration : IRegistration
    {
        public IEnumerable<ServiceDescriptor> ResolveServiceDescriptors(Type implementationType, ServiceLifetime lifetime)
        {
            return implementationType
                .GetInterfaces()
                .Select(x =>
                {
                    var factory = RegistrationHelper.CreateFactory(implementationType);

                    return new ServiceDescriptor(typeof(Func<>).MakeGenericType(x), factory, lifetime);
                });
        }
    }
}