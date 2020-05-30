using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal class SelfRegistration : IRegistration
    {
        public IEnumerable<Type> ResolveServiceTypes(Type implementationType)
        {
            yield return implementationType;
        }
    }
}