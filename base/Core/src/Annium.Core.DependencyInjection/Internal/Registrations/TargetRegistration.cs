using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Registrations
{
    internal class TargetRegistration : IRegistration
    {
        private readonly Type _serviceType;

        public TargetRegistration(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public IEnumerable<Type> ResolveServiceTypes(Type implementationType)
        {
            yield return _serviceType;
        }
    }
}