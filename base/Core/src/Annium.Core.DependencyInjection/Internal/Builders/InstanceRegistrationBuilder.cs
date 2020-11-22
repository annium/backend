using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class InstanceRegistrationBuilder : IInstanceRegistrationBuilderBase
    {
        private readonly Type _type;
        private readonly object _instance;
        private readonly Registrator _registrator;
        private readonly List<IRegistration> _registrations = new();


        public InstanceRegistrationBuilder(
            Type type,
            object instance,
            Action<IServiceDescriptor> register
        )
        {
            _type = type;
            _instance = instance;
            _registrator = new Registrator(register);
        }

        public IInstanceRegistrationBuilderBase AsSelf() =>
            WithRegistration(new InstanceRegistration(_type, _instance));

        public IInstanceRegistrationBuilderBase As(Type serviceType) =>
            WithRegistration(new InstanceRegistration(serviceType, _instance));

        public IInstanceRegistrationBuilderBase AsKeyedSelf<TKey>(TKey key) where TKey : notnull =>
            WithRegistration(new InstanceKeyedRegistration(_type, _instance, typeof(TKey), key));

        public IInstanceRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key) where TKey : notnull =>
            WithRegistration(new InstanceKeyedRegistration(serviceType, _instance, typeof(TKey), key));

        public IInstanceRegistrationBuilderBase AsSelfFactory() =>
            WithRegistration(new InstanceFactoryRegistration(_type, _instance));

        public IInstanceRegistrationBuilderBase AsFactory(Type serviceType) =>
            WithRegistration(new InstanceFactoryRegistration(serviceType, _instance));

        public IInstanceRegistrationBuilderBase AsKeyedSelfFactory<TKey>(TKey key) where TKey : notnull =>
            WithRegistration(new InstanceKeyedFactoryRegistration(_type, _instance, typeof(TKey), key));

        public IInstanceRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key) where TKey : notnull =>
            WithRegistration(new InstanceKeyedFactoryRegistration(serviceType, _instance, typeof(TKey), key));

        public void Singleton() => _registrator.Register(_registrations, ServiceLifetime.Singleton);

        private IInstanceRegistrationBuilderBase WithRegistration(IRegistration registration)
        {
            _registrations.Add(registration);

            return this;
        }
    }
}