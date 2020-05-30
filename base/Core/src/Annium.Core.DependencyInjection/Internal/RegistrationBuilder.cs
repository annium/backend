using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Registrations;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection.Internal
{
    public class RegistrationBuilder : IRegistrationBuilder
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
            _types = types;
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

        public IRegistrationBuilder AsImplementedInterfaces()
        {
            _registrations.Add(new InterfacesRegistration());

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
            foreach (var serviceType in _registrations.SelectMany(x => x.ResolveServiceTypes(implementationType)))
                _services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        }
    }
}