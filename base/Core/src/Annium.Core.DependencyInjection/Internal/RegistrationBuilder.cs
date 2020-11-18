using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Registrations;
using Annium.Core.DependencyInjection.New;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal
{
    internal class RegistrationBuilder : IRegistrationBuilder
    {
        private readonly IServiceCollection _services;
        private readonly ICollection<IRegistration> _registrations = new List<IRegistration>();
        private IEnumerable<Type> _types;
        private bool _hasRegistered;

        public RegistrationBuilder(
            IServiceCollection services,
            IEnumerable<Type> types
        )
        {
            _services = services;
            _types = types
                .Where(x =>
                    !x.IsGenericTypeDefinition &&
                    !x.IsAbstract &&
                    !x.IsInterface
                )
                .ToArray();
        }

        public IRegistrationBuilder Where(Func<Type, bool> predicate)
        {
            _types = _types.Where(predicate);

            return this;
        }

        public IRegistrationBuilder As<T>()
        {
            _registrations.Add(new TargetRegistration(typeof(T)));

            return this;
        }

        public IRegistrationBuilder As(Type serviceType)
        {
            _registrations.Add(new TargetRegistration(serviceType));

            return this;
        }

        public IRegistrationBuilder AsSelf()
        {
            _registrations.Add(new SelfRegistration());

            return this;
        }

        public IRegistrationBuilder AsSelfFactory()
        {
            _registrations.Add(new SelfFactoryRegistration());

            return this;
        }

        public IRegistrationBuilder AsImplementedInterfaces()
        {
            _registrations.Add(new InterfacesRegistration());

            return this;
        }

        public IRegistrationBuilder AsImplementedInterfacesFactories()
        {
            _registrations.Add(new InterfacesFactoriesRegistration());

            return this;
        }

        public void InstancePerScope() => Register(ServiceLifetime.Scoped);

        public void SingleInstance() => Register(ServiceLifetime.Singleton);

        public void InstancePerDependency() => Register(ServiceLifetime.Transient);

        private void Register(ServiceLifetime lifetime)
        {
            if (_hasRegistered)
                throw new InvalidOperationException("Registration already done");
            _hasRegistered = true;

            foreach (var implementationType in _types)
            {
                _services.AddChecked(new ServiceDescriptor(implementationType, implementationType, lifetime));

                var descriptors = _registrations
                    .SelectMany(x => x.ResolveServiceDescriptors(implementationType, lifetime))
                    .ToArray();
                foreach (var descriptor in descriptors)
                    _services.AddChecked(descriptor);
            }
        }
    }
}