using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class FactoryRegistrationBuilder : IFactoryRegistrationBuilderBase
    {
        private readonly Type _type;
        private readonly Func<IServiceProvider, object> _factory;
        private readonly Registrator _registrator;
        private readonly List<IRegistration> _registrations = new();

        public FactoryRegistrationBuilder(
            Type type,
            Func<IServiceProvider, object> factory,
            Action<IServiceDescriptor> register
        )
        {
            _type = type;
            _factory = factory;
            _registrator = new Registrator(register);
        }

        public IFactoryRegistrationBuilderBase AsSelf() =>
            WithRegistration(new FactoryRegistration(_type, _factory));

        public IFactoryRegistrationBuilderBase As(Type serviceType) =>
            WithRegistration(new FactoryRegistration(serviceType, _factory));

        public IFactoryRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull =>
            WithRegistration(new KeyedFactoryRegistration(_type, _factory, typeof(TKey), key));

        public IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull =>
            WithRegistration(new KeyedFactoryRegistration(serviceType, _factory, typeof(TKey), key));

        public void Scoped() => Register(ServiceLifetime.Scoped);
        public void Singleton() => Register(ServiceLifetime.Singleton);
        public void Transient() => Register(ServiceLifetime.Transient);
        private void Register(ServiceLifetime lifetime) => _registrator.Register(_registrations, lifetime);

        private IFactoryRegistrationBuilderBase WithRegistration(IRegistration registration)
        {
            _registrations.Add(registration);

            return this;
        }
    }
}