using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal interface IRegistration
    {
        IEnumerable<Type> ResolveServiceTypes(Type implementationType);
    }
}