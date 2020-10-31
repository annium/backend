using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal class SelfFactoryRegistration : IRegistration
    {
        public IEnumerable<ServiceDescriptor> ResolveServiceDescriptors(Type implementationType, ServiceLifetime lifetime)
        {
            var factory = RegistrationHelper.CreateFactory(implementationType);

            yield return new ServiceDescriptor(
                typeof(Func<>).MakeGenericType(implementationType),
                factory,
                lifetime
            );
        }
    }
}