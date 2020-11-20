using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class BulkRegistrationBuilder : IBulkRegistrationBuilderBase
    {
        private readonly Action<IServiceDescriptor> _register;
        private readonly List<BulkRegistrationUnit> _units;

        public BulkRegistrationBuilder(IEnumerable<Type> types, Action<IServiceDescriptor> register)
        {
            _register = register;
            _units = new List<BulkRegistrationUnit>(types.Select(x => new BulkRegistrationUnit(x)));
        }

        public IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate)
        {
            _units.RemoveAll(x => predicate(x.Type));

            return this;
        }

        public IBulkRegistrationBuilderTarget AsSelf()
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget As<T>()
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsSelfKeyed<TKey>(Func<Type, TKey> getKey)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsKeyed<TKey>(Type serviceType, Func<Type, TKey> getKey)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsKeyed<T, TKey>(Func<Type, TKey> getKey)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsSelfFactory()
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsFactory<T>()
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsSelfKeyedFactory<TKey>(Func<Type, TKey> getKey)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsKeyedFactory<TKey>(Type serviceType, Func<Type, TKey> getKey)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsKeyedFactory<T, TKey>(Func<Type, TKey> getKey)
        {
            throw new NotImplementedException();
        }

        public void Scoped()
        {
            throw new NotImplementedException();
        }

        public void Singleton()
        {
            throw new NotImplementedException();
        }

        public void Transient()
        {
            throw new NotImplementedException();
        }
    }
}