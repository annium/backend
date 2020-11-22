using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class SingleRegistrationBuilder : ISingleRegistrationBuilderBase
    {
        private readonly Type _type;
        private readonly Registrator _registrator;
        private readonly List<IRegistration> _registrations = new();

        public SingleRegistrationBuilder(
            Type type,
            Action<IServiceDescriptor> register
        )
        {
            _type = type;
            _registrator = new Registrator(register);
        }


        public ISingleRegistrationBuilderBase AsSelf() =>
            WithRegistration(new TargetRegistration(_type, _type));

        public ISingleRegistrationBuilderBase As(Type serviceType) =>
            WithRegistration(new TargetRegistration(serviceType, _type));

        public ISingleRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull =>
            WithRegistration(new TargetKeyedRegistration(_type, _type, typeof(TKey), key));

        public ISingleRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull =>
            WithRegistration(new TargetKeyedRegistration(serviceType, _type, typeof(TKey), key));

        public ISingleRegistrationBuilderBase AsSelfFactory() =>
            WithRegistration(new TargetFactoryRegistration(_type, _type));

        public ISingleRegistrationBuilderBase AsFactory(Type serviceType) =>
            WithRegistration(new TargetFactoryRegistration(serviceType, _type));

        public ISingleRegistrationBuilderBase AsKeyedSelfFactory<TKey>(TKey key) where TKey : notnull =>
            WithRegistration(new TargetKeyedFactoryRegistration(_type, _type, typeof(TKey), key));

        public ISingleRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key) where TKey : notnull =>
            WithRegistration(new TargetKeyedFactoryRegistration(serviceType, _type, typeof(TKey), key));

        public void Scoped() => Register(ServiceLifetime.Scoped);
        public void Singleton() => Register(ServiceLifetime.Singleton);
        public void Transient() => Register(ServiceLifetime.Transient);

        private void Register(ServiceLifetime lifetime) => _registrator
            .Register(new[] { new TargetRegistration(_type, _type) }.Concat(_registrations).ToArray(), lifetime);

        private ISingleRegistrationBuilderBase WithRegistration(IRegistration registration)
        {
            _registrations.Add(registration);

            return this;
        }
    }
}