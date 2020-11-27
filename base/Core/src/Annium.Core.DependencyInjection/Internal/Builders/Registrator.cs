using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class Registrator
    {
        private readonly Action<IServiceDescriptor> _register;
        private bool _hasRegistered;

        public Registrator(Action<IServiceDescriptor> register)
        {
            _register = register;
        }

        public void Register(
            IReadOnlyCollection<IRegistration> registrations,
            ServiceLifetime lifetime
        )
        {
            if (_hasRegistered)
                throw new InvalidOperationException("Registration already done");
            _hasRegistered = true;

            if (registrations.Count == 0)
                throw new InvalidOperationException("No registration specified");

            var descriptors = registrations
                .SelectMany(x => x.ResolveServiceDescriptors(lifetime))
                .ToArray();
            foreach (var descriptor in descriptors)
                _register(descriptor);
        }
    }
}