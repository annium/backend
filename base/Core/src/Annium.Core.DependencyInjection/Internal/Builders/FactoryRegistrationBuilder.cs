using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class FactoryRegistrationBuilder : IFactoryRegistrationBuilderBase
    {
        private readonly Type _type;
        private readonly Func<IServiceProvider, object> _factory;
        private readonly Registrar _registrar;
        private readonly RegistrationsCollection _registrations = new();

        public FactoryRegistrationBuilder(
            Type type,
            Func<IServiceProvider, object> factory,
            Registrar registrar
        )
        {
            _type = type;
            _factory = factory;
            _registrar = registrar;
        }

        public IFactoryRegistrationBuilderBase AsSelf() =>
            WithRegistration(new FactoryRegistration(_type, _factory));

        public IFactoryRegistrationBuilderBase As(Type serviceType) =>
            WithRegistration(new FactoryRegistration(serviceType, _factory));

        public IFactoryRegistrationBuilderBase AsInterfaces() =>
            WithRegistrations(_type.GetInterfaces().Select(x => new FactoryRegistration(x, _factory)));

        public IFactoryRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull =>
            WithRegistration(new FactoryKeyedRegistration(_type, _factory, typeof(TKey), key));

        public IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull =>
            WithRegistration(new FactoryKeyedRegistration(serviceType, _factory, typeof(TKey), key));

        public void In(ServiceLifetime lifetime) => _registrar.Register(_registrations, lifetime);
        public void Scoped() => In(ServiceLifetime.Scoped);
        public void Singleton() => In(ServiceLifetime.Singleton);
        public void Transient() => In(ServiceLifetime.Transient);

        private IFactoryRegistrationBuilderBase WithRegistrations(IEnumerable<IRegistration> registrations)
        {
            _registrations.Init();
            _registrations.AddRange(registrations);

            return this;
        }

        private IFactoryRegistrationBuilderBase WithRegistration(IRegistration registration)
        {
            _registrations.Init();
            _registrations.Add(registration);

            return this;
        }
    }
}