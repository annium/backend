using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class BulkRegistrationBuilder : IBulkRegistrationBuilderBase
    {
        private readonly List<Type> _types;
        private readonly Registrator _registrator;
        private readonly List<IRegistration> _registrations = new();

        public BulkRegistrationBuilder(IEnumerable<Type> types, Action<IServiceDescriptor> register)
        {
            _types = types.ToList();
            _registrator = new Registrator(register);
        }

        public IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate)
        {
            _types.RemoveAll(x => !predicate(x));

            return this;
        }

        public IBulkRegistrationBuilderTarget AsSelf() =>
            WithRegistration(type => new TargetRegistration(type, type));

        public IBulkRegistrationBuilderTarget As(Type serviceType) =>
            WithRegistration(type => new TargetRegistration(serviceType, type));

        public IBulkRegistrationBuilderTarget AsKeyedSelf<TKey>(Func<Type, TKey> getKey) where TKey : notnull =>
            WithRegistration(type => new TargetKeyedRegistration(type, type, typeof(TKey), getKey(type)));

        public IBulkRegistrationBuilderTarget AsKeyed<TKey>(Type serviceType, Func<Type, TKey> getKey) where TKey : notnull =>
            WithRegistration(type => new TargetKeyedRegistration(serviceType, type, typeof(TKey), getKey(type)));

        public IBulkRegistrationBuilderTarget AsSelfFactory() =>
            WithRegistration(type => new TargetFactoryRegistration(type, type));

        public IBulkRegistrationBuilderTarget AsFactory(Type serviceType) =>
            WithRegistration(type => new TargetFactoryRegistration(serviceType, type));

        public IBulkRegistrationBuilderTarget AsKeyedSelfFactory<TKey>(Func<Type, TKey> getKey) where TKey : notnull =>
            WithRegistration(type => new TargetKeyedFactoryRegistration(type, type, typeof(TKey), getKey(type)));

        public IBulkRegistrationBuilderTarget AsKeyedFactory<TKey>(Type serviceType, Func<Type, TKey> getKey) where TKey : notnull =>
            WithRegistration(type => new TargetKeyedFactoryRegistration(serviceType, type, typeof(TKey), getKey(type)));

        public void Scoped() => Register(ServiceLifetime.Scoped);
        public void Singleton() => Register(ServiceLifetime.Singleton);
        public void Transient() => Register(ServiceLifetime.Transient);

        private void Register(ServiceLifetime lifetime) => _registrator
            .Register(_types.Select(x => new TargetRegistration(x, x)).ToArray().Concat(_registrations).ToArray(), lifetime);

        private IBulkRegistrationBuilderTarget WithRegistration(Func<Type, IRegistration> createRegistration)
        {
            _registrations.AddRange(_types.Select(createRegistration));

            return this;
        }
    }
}