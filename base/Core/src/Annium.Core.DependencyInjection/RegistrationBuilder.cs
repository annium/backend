using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Core.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public class RegistrationBuilder : IRegistrationBuilder
    {
        private readonly IServiceCollection services;
        private IEnumerable<Type> types;
        private RegistrationType registrationType;
        private bool hasRegistered = false;

        public RegistrationBuilder(
            IServiceCollection services,
            IEnumerable<Type> types
        )
        {
            this.services = services;
            this.types = types;
        }

        public IRegistrationBuilder Where(Func<Type, bool> predicate)
        {
            types = types.Where(predicate);

            return this;
        }

        public IRegistrationBuilder AsImplementedInterfaces()
        {
            registrationType = RegistrationType.ImplementedInterfaces;

            return this;
        }

        public void RegisterScoped() => Register(ServiceLifetime.Scoped);

        public void RegisterSingleton() => Register(ServiceLifetime.Singleton);

        public void RegisterTransient() => Register(ServiceLifetime.Transient);

        private void Register(ServiceLifetime lifetime)
        {
            if (hasRegistered)
                throw new InvalidOperationException($"Can't run same registration more than once");
            hasRegistered = true;

            foreach (var implementationType in types)
                foreach (var serviceType in GetServiceTypes(implementationType))
                    services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        }

        private IEnumerable<Type> GetServiceTypes(Type implementationType)
        {
            if (registrationType == RegistrationType.Self)
                yield return implementationType;
            else if (registrationType == RegistrationType.ImplementedInterfaces)
                foreach (var serviceType in implementationType.GetInterfaces())
                    yield return serviceType;
        }
    }
}