using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal class InterfacesRegistration : IRegistration
    {
        public IEnumerable<Type> ResolveServiceTypes(Type implementationType) => implementationType.GetInterfaces();
    }
}