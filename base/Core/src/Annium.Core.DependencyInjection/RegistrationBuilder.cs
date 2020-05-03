using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class RegistrationBuilder : IRegistrationBuilder
    {
        private readonly IServiceCollection _services;
        private IEnumerable<Type> _types;
        private RegistrationType _registrationType;
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

        public IRegistrationBuilder AsImplementedInterfaces()
        {
            _registrationType = RegistrationType.ImplementedInterfaces;

            return this;
        }

        public void RegisterScoped() => Register(ServiceLifetime.Scoped);

        public void RegisterSingleton() => Register(ServiceLifetime.Singleton);

        public void RegisterTransient() => Register(ServiceLifetime.Transient);

        private void Register(ServiceLifetime lifetime)
        {
            if (_hasRegistered)
                throw new InvalidOperationException($"Can't run same registration more than once");
            _hasRegistered = true;

            foreach (var implementationType in _types)
                foreach (var serviceType in GetServiceTypes(implementationType))
                    _services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        }

        private IEnumerable<Type> GetServiceTypes(Type implementationType)
        {
            if (_registrationType == RegistrationType.Self)
                yield return implementationType;
            else if (_registrationType == RegistrationType.ImplementedInterfaces)
                foreach (var serviceType in implementationType.GetInterfaces())
                    yield return serviceType;
        }
    }
}